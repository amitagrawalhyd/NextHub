# NestHub — Feature Testing Guide

This walks through every feature using the seeded demo data (`DatabaseSeeder`). All commands
assume PowerShell and the API running locally at `http://localhost:5299`. Swagger UI is available
at `http://localhost:5299/swagger` if you prefer clicking through requests instead of the CLI.

## 0. Setup

```powershell
# 1. Seed the database (safe to re-run — no-ops if already seeded)
dotnet run --project src\NestHub.API\NestHub.API.csproj -- --seed

# 2. Run the API (separate terminal, leave running)
dotnet run --project src\NestHub.API\NestHub.API.csproj --urls http://localhost:5299

# 3. Run the Admin portal (separate terminal, leave running)
dotnet run --project src\NestHub.Admin\NestHub.Admin.csproj --urls http://localhost:5300
```

All seeded accounts use password **`Passw0rd!23`**.

| Role | Phone | Who |
|---|---|---|
| Admin | `9000000001` | — |
| Resident | `9000000101` | Amit Agrawal — Lakeview Residency, A-204 |
| Resident | `9000000102` | Priya Sharma — Lakeview Residency |
| Resident | `9000000103` | Ravi Kumar — Green Meadows |
| Vendor (approved, Premium, ID Verified) | `9000000201` | Sharma Plumbing Works |
| Vendor (approved, Free, Society Regular) | `9000000202` | BrightSpark Electricians |
| Vendor (pending approval) | `9000000205` | QuickFix Appliance Repair |

A PowerShell helper used throughout — paste this once per session:

```powershell
$base = "http://localhost:5299/api"

function Login($phone) {
    (Invoke-RestMethod -Uri "$base/auth/login" -Method Post -ContentType "application/json" `
        -Body (@{ phoneNumber = $phone; password = "Passw0rd!23" } | ConvertTo-Json)).token
}

function Api($path, $method = "Get", $body = $null, $token = $null) {
    $headers = @{}
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    if ($body) {
        Invoke-RestMethod -Uri "$base$path" -Method $method -ContentType "application/json" -Headers $headers -Body ($body | ConvertTo-Json)
    } else {
        Invoke-RestMethod -Uri "$base$path" -Method $method -Headers $headers
    }
}

$adminToken    = Login "9000000001"
$amitToken     = Login "9000000101"   # resident
$plumberToken  = Login "9000000201"   # approved vendor
$sparkToken    = Login "9000000202"   # approved vendor
$quickfixToken = Login "9000000205"   # pending vendor
```

---

## 1. Auth (Registration, OTP-less login, Verification)

```powershell
# Register a brand-new resident
$newUserId = Api "/auth/register" Post @{ phoneNumber = "9999912345"; password = "Passw0rd!23"; userType = 1 } # 1=Resident, 2=Vendor, 3=Admin

# Mark them verified (simulates OTP confirmation)
Api "/auth/verify" Post @{ userId = $newUserId }

# Log in as them
$newToken = Login "9999912345"
```
**Expect:** a new GUID from register, no error from verify, a JWT string from login.

---

## 2. Societies

```powershell
# Public: list active societies (no auth needed)
Api "/societies"

# Admin-only: register a new society
Api "/societies" Post @{ name = "Prestige Lakeside"; address = "Nallagandla, Hyderabad"; latitude = 17.46; longitude = 78.30 } -token $adminToken

# Confirm a non-admin gets rejected
try { Api "/societies" Post @{ name = "Should Fail"; address = "x" } -token $amitToken } catch { Write-Output "Correctly rejected: $($_.Exception.Response.StatusCode)" }
```
**Expect:** 3 seeded societies listed; new one appears after creation; resident attempt returns `403 Forbidden`.

---

## 3. Residents

```powershell
$amitProfile = Api "/residents/me" Get -token $amitToken
$amitProfile   # Id, UserId, SocietyId (Lakeview), BlockNumber=A, FlatNumber=204, Name=Amit Agrawal

# Register a resident profile for the brand-new user from step 1
$societies = Api "/societies"
$resident = Api "/residents" Post @{ userId = $newUserId; societyId = $societies[0].id; blockNumber = "Z"; flatNumber = "999"; name = "Test Person" } -token $newToken
```
**Expect:** `Amit`'s profile with his real society/flat; new resident profile created successfully.

---

## 4. Vendors — onboarding, approval, catalogue

```powershell
$plumberId = (Api "/vendors/search?query=plumb")[0].id

# Vendor self-service profile
Api "/vendors/me" Get -token $plumberToken

# Admin: see the pending-approval queue (should include QuickFix Appliance Repair)
Api "/vendors/pending-approval" -token $adminToken

# Admin: approve QuickFix
$quickfixId = (Api "/vendors/pending-approval" -token $adminToken)[0].id
Api "/vendors/$quickfixId/approve" Post -token $adminToken

# Vendor: add a service (PricingType: 1=Fixed, 2=Hourly, 3=StartingFrom, 4=ContactForQuote)
Api "/vendors/$plumberId/services" Post @{ title = "Geyser Installation"; description = "Full geyser install"; category = "Plumbing"; pricingType = 1; amount = 800 } -token $plumberToken

# Vendor: attempt to upgrade own subscription
Api "/vendors/$plumberId/upgrade-subscription" Post -token $plumberToken

# Admin: award a trust badge (Badge: None/IdVerified/SocietyRegular)
Api "/vendors/$plumberId/trust-badge" Post @{ badge = "SocietyRegular" } -token $adminToken

# Public: full-text-ish smart search
Api "/vendors/search?query=electrician"
Api "/vendors/search?category=Plumbing"
```
**Expect:** QuickFix disappears from the pending queue after approval; new service appears on
`GET /vendors/{id}`; search returns relevance-ranked results (see §8 for how ranking works).

**Free-tier limit check** (a real domain rule worth exercising): register a brand-new vendor and
try adding a 4th service before upgrading — the 4th should fail with a `400` from
`FreeTierServiceLimitExceededException`.

---

## 5. Reviews — the "Neighbor-Verified" feature

This is the core trust mechanic — reviews are filtered to the *viewer's own society*.

```powershell
$plumberId = (Api "/vendors/search?query=plumb")[0].id
$lakeviewSocietyId = $amitProfile.societyId

# Amit (Lakeview) sees only Lakeview neighbors' reviews for the plumber
Api "/reviews?vendorId=$plumberId&societyId=$lakeviewSocietyId" -token $amitToken
# Expect: 2 reviews (from Amit and Priya, both Lakeview) — Ravi's (Green Meadows) review and
# the flagged review are NOT in this list.

# Submit a new review — sentiment is auto-scored by the AI service
$review = Api "/reviews" Post @{ residentId = $amitProfile.id; vendorId = $plumberId; societyId = $lakeviewSocietyId; rating = 5; comment = "Absolutely fantastic, on time and professional!" } -token $amitToken
$review.sentimentScore   # should be positive (close to 1)

# Confirm the vendor's average rating recalculated
(Api "/vendors/$plumberId").averageRating
```
**Expect:** review count filtered strictly by `societyId`; `sentimentScore` reflects the positive
wording; average rating updates immediately.

---

## 6. Review Moderation (Admin)

```powershell
# Admin: view flagged reviews (the seeded "Rude behavior..." review should be here)
$flagged = Api "/reviews/flagged" -token $adminToken
$flagged

# Admin: remove it
Api "/reviews/$($flagged[0].id)" Delete -token $adminToken

# Flag a different review as an admin (simulating a resident/vendor report)
$reviewToFlag = (Api "/reviews?vendorId=$plumberId&societyId=$lakeviewSocietyId" -token $adminToken)[0]
Api "/reviews/$($reviewToFlag.id)/flag" Post -token $adminToken
```
**Expect:** flagged list shows the seeded bad review; it's gone from both `/reviews/flagged` and
the vendor's public review feed after removal.

---

## 7. SOS Requests — urgent broadcast + claim

```powershell
$raviToken = Login "9000000103"
$raviProfile = Api "/residents/me" -token $raviToken

# Resident raises an SOS
$sos = Api "/sos-requests" Post @{ residentId = $raviProfile.id; societyId = $raviProfile.societyId; category = "Plumbing"; description = "Burst pipe flooding the kitchen!" } -token $raviToken
$sos.status   # "Open"

# Vendor checks open leads matching their category + society
Api "/sos-requests/open?societyId=$($raviProfile.societyId)&category=Plumbing" -token $plumberToken

# Vendor claims it
$claim = Api "/sos-requests/$($sos.id)/claim" Post @{ vendorId = $plumberId } -token $plumberToken
$claim.claimedDateTimeUtc

# Confirm status flips to Claimed, and a second claim attempt by a different vendor still works
# (multiple vendors CAN claim the same request — the resident picks who to actually engage)
Api "/sos-requests/$($sos.id)/claim" Post @{ vendorId = (Api "/vendors/search?query=spark")[0].id } -token $sparkToken

# Resident closes the request once resolved
Api "/sos-requests/$($sos.id)/close" Post -token $raviToken

# Closing twice should fail (400 — SosRequestClosedException)
try { Api "/sos-requests/$($sos.id)/close" Post -token $raviToken } catch { Write-Output "Correctly rejected: already closed" }
```
**Expect:** the seeded open SOS (`9000000103`'s "Leaking pipe...") and claimed SOS
(electrician, already claimed) are visible via the DB; new requests broadcast in real time — see
§9 for the SignalR push verification.

---

## 8. Smart Search & Sentiment (AI features)

The "AI" is a deterministic local implementation (`LocalAiService`) — no external API key needed,
which makes it fully testable offline:

```powershell
# Keyword-weighted ranking: business name matches score higher than bio matches
Api "/vendors/search?query=Sharma"      # matches business name directly
Api "/vendors/search?query=tuition"     # matches service title/category for MathGenius

# Sentiment scoring: try a clearly negative comment
$negativeReview = Api "/reviews" Post @{ residentId = $amitProfile.id; vendorId = (Api "/vendors/search?query=spark")[0].id; societyId = $lakeviewSocietyId; rating = 1; comment = "Terrible and rude, avoid this vendor." } -token $amitToken
$negativeReview.sentimentScore   # should be negative (close to -1)
```
**Expect:** relevant vendors surface for both name- and service-based queries; sentiment score
sign matches the comment's tone.

---

## 9. Real-time SOS push (SignalR)

To see the live broadcast (not just the REST polling in §7), open two PowerShell windows:

**Window A — vendor listening:**
```powershell
# Requires a SignalR client; simplest is the Mobile app's Vendor > SOS Leads tab (see §11),
# or use the `Microsoft.AspNetCore.SignalR.Client` package in a small test script that connects
# to ws://localhost:5299/hubs/sos, joins group JoinSocietyCategoryGroup(societyId, "Plumbing"),
# and logs the "SosRequestCreated" event.
```

**Window B — resident triggers it:**
```powershell
Api "/sos-requests" Post @{ residentId = $raviProfile.id; societyId = $raviProfile.societyId; category = "Plumbing"; description = "Live test!" } -token $raviToken
```
**Expect:** Window A receives the `SosRequestCreated` event within milliseconds, before Window B's
HTTP call even returns its response body (the broadcast happens after the DB save inside the
command handler).

The easiest way to see this without writing a client is via the **Mobile app** — see §11.

---

## 10. Analytics Dashboard

```powershell
Api "/analytics/events" Post @{ vendorId = $plumberId; actionType = "ProfileView" }   # no auth — public event tracking
Api "/analytics/events" Post @{ vendorId = $plumberId; actionType = "CallClick" }
Api "/analytics/events" Post @{ vendorId = $plumberId; actionType = "WhatsAppClick" }

$from = (Get-Date).AddDays(-30).ToString("o")
$to = (Get-Date).ToString("o")
Api "/analytics/vendors/$plumberId/dashboard?fromUtc=$from&toUtc=$to" -token $plumberToken
```
**Expect:** counts reflect the 251 seeded events plus the 3 you just recorded; a vendor cannot
fetch another vendor's dashboard's — try `$sparkToken` against `$plumberId`'s dashboard and
confirm... (note: the current authorization only checks the `Vendor` role, not resource
ownership — this is a known simplification worth tightening if this goes further than a demo).

---

## 11. Admin Portal (browser: `http://localhost:5300`)

1. **Login** (`/Account/Login`) — phone `9000000001`, password `Passw0rd!23`.
2. **Vendor Approvals** (default landing page) — approve `QuickFix Appliance Repair` if not
   already done; award a Trust Badge to any approved vendor from the dropdown + "Award" button.
3. **Categories** — add a new category (e.g. "Carpentry"), toggle one Active/Inactive, delete one.
4. **Review Moderation** — the seeded flagged review ("Rude behavior...") should appear; click
   "Remove" and confirm it disappears.
5. **Societies** — register a new society via the form; confirm it appears in the table below.
6. **System Audit** — should list the pending-vendor and flagged-review entries live from the
   database (this view is a composite read, not a stored audit log — see code comments).
7. **Sign out** — confirm you're redirected to the login page and can no longer reach any page
   without re-authenticating (try navigating directly to `/Vendors` while signed out).

---

## 12. Mobile App (Android emulator/device)

Update `MauiProgram.ApiBaseAddress` if testing on a physical device (it defaults to the Android
emulator's `10.0.2.2` alias for your machine's `localhost`).

**As a Resident (`9000000101` / `Passw0rd!23`):**
1. Login screen → sign in → lands on Resident tabs (Home/Search/SOS) since Amit already has a
   profile (no onboarding screen).
2. **Search tab** — search "plumb" → tap "Sharma Plumbing Works" → vendor profile opens.
3. On the vendor profile: tap **Call** — the one-time "Contacting the vendor directly..."
   disclaimer should appear before the dialer opens; tap **WhatsApp** — same disclaimer gate
   (only shown once per install; check by tapping the other button afterward — no second popup).
4. Scroll to reviews — should show only Lakeview neighbors' reviews for this vendor.
5. Submit a review with the Stepper + Editor, tap "Submit review" — it should appear at the top
   of the list immediately.
6. **SOS tab** — pick a category, enter a description, tap "Send SOS" — confirmation message
   appears.

**As a Vendor (`9000000201` / `Passw0rd!23`):**
1. Login → lands on Vendor tabs (Dashboard/SOS Leads).
2. **Dashboard** — bar chart should render three bars (Profile Views / Calls / WhatsApp) with
   real seeded numbers.
3. **SOS Leads** — should show any open SOS request matching the vendor's first listed service
   category and the first society returned by the API. Tap "Claim this lead" and confirm it
   disappears from the list.
4. To see the *real-time* push: leave this tab open, then (from a PowerShell window or the
   Resident app on a second device/emulator) create a new SOS request in the same category/society
   — it should appear in the list without refreshing.

**New user path** — register a brand-new account from the Login screen's "Create an account"
link, choosing Resident or Vendor: after login you should land on the onboarding page (pick a
society / enter business details) instead of the tabs, since no profile exists yet.

---

## 13. Things that should fail (negative-path sanity checks)

| Action | Expected result |
|---|---|
| Login with wrong password | `401 Unauthorized` |
| Non-admin calling `POST /societies` | `403 Forbidden` |
| Submit a review with `rating = 6` | `400` — FluentValidation error |
| Add a 4th service on a Free-tier vendor | `400` — `FreeTierServiceLimitExceededException` |
| Add a service before vendor is approved | `400` — `VendorNotApprovedException` |
| Same vendor claims the same SOS request twice | `400` — `SosRequestAlreadyClaimedByVendorException` |
| Close an already-closed SOS request | `400` — `SosRequestClosedException` |
| `GET /residents/me` for a user with no resident profile | `204 No Content` (not an error) |
