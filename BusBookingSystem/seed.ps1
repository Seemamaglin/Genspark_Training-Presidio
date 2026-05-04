$API = "http://localhost:5000/api"
$ErrorActionPreference = "SilentlyContinue"

function Post-Api($path, $body, $tok) {
    $h = @{ "Content-Type" = "application/json" }
    if ($tok) { $h["Authorization"] = "Bearer $tok" }
    try {
        return Invoke-RestMethod -Method POST -Uri "$API$path" -Headers $h -Body ($body | ConvertTo-Json -Depth 5) -ErrorAction Stop
    } catch {
        Write-Host "  WARN POST $path : $($_.Exception.Message.Substring(0,[Math]::Min(80,$_.Exception.Message.Length)))"
        return $null
    }
}

function Get-Api($path, $tok) {
    $h = @{ "Authorization" = "Bearer $tok" }
    try { return Invoke-RestMethod -Method GET -Uri "$API$path" -Headers $h -ErrorAction Stop }
    catch { return $null }
}

Write-Host ""
Write-Host "=== Bus Booking Seed Script ==="

# 1. Admin login
$login = Post-Api "/Account/login" @{ email = "admin@busbooking.com"; password = "Admin@1234" } $null
if (-not $login) { Write-Host "Cannot login as admin. Is the backend running?"; exit 1 }
$adminToken = $login.token
Write-Host "Admin logged in OK"

# 2. Create routes
$routeList = @(
    @{ source = "Chennai";     destination = "Coimbatore"   },
    @{ source = "Coimbatore";  destination = "Chennai"      },
    @{ source = "Chennai";     destination = "Madurai"      },
    @{ source = "Madurai";     destination = "Chennai"      },
    @{ source = "Chennai";     destination = "Pondicherry"  },
    @{ source = "Pondicherry"; destination = "Chennai"      },
    @{ source = "Chennai";     destination = "Salem"        },
    @{ source = "Salem";       destination = "Chennai"      },
    @{ source = "Chennai";     destination = "Trichy"       },
    @{ source = "Trichy";      destination = "Chennai"      },
    @{ source = "Coimbatore";  destination = "Madurai"      },
    @{ source = "Madurai";     destination = "Coimbatore"   },
    @{ source = "Chennai";     destination = "Tirunelveli"  },
    @{ source = "Tirunelveli"; destination = "Chennai"      },
    @{ source = "Coimbatore";  destination = "Bangalore"    },
    @{ source = "Bangalore";   destination = "Coimbatore"   },
    @{ source = "Chennai";     destination = "Vellore"      },
    @{ source = "Madurai";     destination = "Trichy"       }
)

Write-Host ""
Write-Host "Creating routes..."
$routeIds = @{}

# Fetch existing routes first
$existing = Get-Api "/Admin/routes" $adminToken
if ($existing) {
    foreach ($er in $existing) {
        $k = "$($er.source)_$($er.destination)"
        $routeIds[$k] = $er.id
    }
    Write-Host "  Found $($existing.Count) existing routes"
}

foreach ($r in $routeList) {
    $k = "$($r.source)_$($r.destination)"
    if ($routeIds.ContainsKey($k)) {
        Write-Host "  Exists: $($r.source) to $($r.destination)"
        continue
    }
    $res = Post-Api "/Admin/routes" $r $adminToken
    if ($res -and $res.id) {
        $routeIds[$k] = $res.id
        Write-Host "  Created: $($r.source) to $($r.destination)"
    }
}
Write-Host "Routes ready: $($routeIds.Count)"

# 3. Register operator accounts
Write-Host ""
Write-Host "Setting up operators..."
$operatorList = @(
    @{ email = "tnexpress@operator.com";  password = "Operator@1234"; name = "TN Express Travels";      phone = "9876543210" },
    @{ email = "southtours@operator.com"; password = "Operator@1234"; name = "South India Tours";        phone = "9876543211" },
    @{ email = "pondylines@operator.com"; password = "Operator@1234"; name = "Pondy Lines";              phone = "9876543212" },
    @{ email = "cbefast@operator.com";    password = "Operator@1234"; name = "Coimbatore Fast Travels";  phone = "9876543213" },
    @{ email = "maduraiexp@operator.com"; password = "Operator@1234"; name = "Madurai Royal Express";    phone = "9876543214" },
    @{ email = "salemstar@operator.com";  password = "Operator@1234"; name = "Salem Star Transport";     phone = "9876543215" }
)

foreach ($op in $operatorList) {
    $tryLogin = Post-Api "/Account/login" @{ email = $op.email; password = $op.password } $null
    if ($tryLogin -and $tryLogin.token) {
        Write-Host "  Exists: $($op.name)"
        Post-Api "/Account/request-operator-upgrade" @{} $tryLogin.token | Out-Null
    } else {
        $reg = Post-Api "/Account/register" @{
            email = $op.email; password = $op.password; name = $op.name
            phoneNumber = $op.phone; age = 38; proof = "DL"
        } $null
        if ($reg -and $reg.token) {
            Write-Host "  Registered: $($op.name)"
            Post-Api "/Account/request-operator-upgrade" @{} $reg.token | Out-Null
        }
    }
}

# 4. Approve all pending operator requests
$pendingOps = Get-Api "/Admin/operator-requests" $adminToken
$routeKeysList = @($routeIds.Keys)
$opIdx = 0
if ($pendingOps -and $pendingOps.Count -gt 0) {
    Write-Host "  Approving $($pendingOps.Count) operator requests..."
    foreach ($req in $pendingOps) {
        $assignRouteId = $routeIds[$routeKeysList[$opIdx % $routeKeysList.Count]]
        $res = Post-Api "/Admin/operators/$($req.id)/approve" @{ routeId = $assignRouteId } $adminToken
        if ($res) { Write-Host "  Approved: $($req.name)" }
        $opIdx++
    }
}

# 5. Register sample passenger users
Write-Host ""
Write-Host "Registering passenger accounts..."
$passengerList = @(
    @{ email = "rahul.kumar@test.com";   name = "Rahul Kumar";     phone = "9500001111" },
    @{ email = "priya.s@test.com";       name = "Priya Shankar";   phone = "9500002222" },
    @{ email = "arun.m@test.com";        name = "Arun Murugan";    phone = "9500003333" },
    @{ email = "divya.r@test.com";       name = "Divya Rajan";     phone = "9500004444" },
    @{ email = "karthik.v@test.com";     name = "Karthik Vel";     phone = "9500005555" },
    @{ email = "meena.b@test.com";       name = "Meena Balan";     phone = "9500006666" },
    @{ email = "suresh.p@test.com";      name = "Suresh Pandi";    phone = "9500007777" },
    @{ email = "lakshmi.n@test.com";     name = "Lakshmi Nair";    phone = "9500008888" },
    @{ email = "vijay.k@test.com";       name = "Vijay Kumar";     phone = "9500009999" },
    @{ email = "anitha.m@test.com";      name = "Anitha Mohan";    phone = "9500010000" }
)
foreach ($p in $passengerList) {
    $reg = Post-Api "/Account/register" @{
        email = $p.email; password = "User@1234"; name = $p.name
        phoneNumber = $p.phone; age = 28; proof = "Aadhaar"
    } $null
    if ($reg) { Write-Host "  Registered: $($p.name)" }
    else       { Write-Host "  Exists: $($p.name)" }
}

# 6. Create buses
Write-Host ""
Write-Host "Creating buses (this may take a while)..."

$busTypes = @("AC Seater", "Non-AC Seater", "Volvo AC", "AC Sleeper", "Semi-Sleeper")
$companyWords = @("Express", "Deluxe", "Royal", "Ultra", "Super", "Premium", "Star", "VIP", "Fast", "Comfort")

function Get-BusPrice($type) {
    switch ($type) {
        "Volvo AC"      { return 600 + (Get-Random -Min 0 -Max 250) }
        "AC Sleeper"    { return 800 + (Get-Random -Min 0 -Max 400) }
        "AC Seater"     { return 400 + (Get-Random -Min 0 -Max 200) }
        "Semi-Sleeper"  { return 480 + (Get-Random -Min 0 -Max 170) }
        "Non-AC Seater" { return 200 + (Get-Random -Min 0 -Max 120) }
        default         { return 400 }
    }
}

function Get-RouteDuration($src, $dst) {
    $p = "$src_$dst"
    $map = @{
        "Chennai_Coimbatore"    = 7; "Coimbatore_Chennai"    = 7
        "Chennai_Madurai"       = 8; "Madurai_Chennai"       = 8
        "Chennai_Pondicherry"   = 3; "Pondicherry_Chennai"   = 3
        "Chennai_Salem"         = 5; "Salem_Chennai"         = 5
        "Chennai_Trichy"        = 6; "Trichy_Chennai"        = 6
        "Coimbatore_Madurai"    = 4; "Madurai_Coimbatore"    = 4
        "Chennai_Tirunelveli"   = 9; "Tirunelveli_Chennai"   = 9
        "Coimbatore_Bangalore"  = 5; "Bangalore_Coimbatore"  = 5
        "Chennai_Vellore"       = 2; "Madurai_Trichy"        = 3
    }
    if ($map.ContainsKey($p)) { return $map[$p] }
    return 6
}

# All departure times to pick from
$allDepTimes = @("05:30","06:00","06:30","07:00","08:00","09:00","10:00","12:00","14:00","15:30","17:00","18:00","19:00","20:00","21:00","21:30","22:00","22:30","23:00","23:30")

$usedRegNums = @{}
$busCount = 0
$today = Get-Date

# Each route gets buses across next 7 days
foreach ($routeKey in $routeIds.Keys) {
    $routeId = $routeIds[$routeKey]
    $parts = $routeKey.Split("_")
    $src = $parts[0]
    $dst = $parts[1]
    $duration = Get-RouteDuration $src $dst

    # Pick 5-6 different departure times for this route
    $timesForRoute = $allDepTimes | Get-Random -Count 6 | Sort-Object

    for ($day = 0; $day -le 6; $day++) {
        $travelDate = $today.AddDays($day).ToString("yyyy-MM-dd")

        # 2-3 buses per day per route
        $numBusesToday = Get-Random -Min 1 -Max 3
        $todayTimes = $timesForRoute | Get-Random -Count $numBusesToday | Sort-Object

        foreach ($depTime in $todayTimes) {
            $busType = $busTypes[(Get-Random -Min 0 -Max $busTypes.Count)]
            $company = $companyWords[(Get-Random -Min 0 -Max $companyWords.Count)]
            $busName = "$src $company Travels"
            $price   = Get-BusPrice $busType
            $seats   = if ($busType -eq "AC Sleeper") { 36 } elseif ($busType -eq "Non-AC Seater") { 52 } else { 40 }

            # Generate unique TN-style reg number
            $attempts = 0
            do {
                $distNum  = Get-Random -Min 1 -Max 99
                $letters  = [char[]]@(65..90 | Get-Random -Count 2) -join ""
                $num4     = Get-Random -Min 1000 -Max 9999
                $regNum   = "TN{0:D2}{1}{2}" -f $distNum, $letters, $num4
                $attempts++
            } while ($usedRegNums.ContainsKey($regNum) -and $attempts -lt 20)
            $usedRegNums[$regNum] = $true

            # Compute arrival time
            $depH = [int]($depTime.Split(":")[0])
            $depM = [int]($depTime.Split(":")[1])
            $arrH = ($depH + $duration) % 24
            $arrTime = "{0:D2}:{1:D2}" -f $arrH, $depM

            $busPayload = @{
                registrationNumber = $regNum
                busName            = $busName
                busType            = $busType
                departureTime      = $depTime
                arrivalTime        = $arrTime
                travelDate         = $travelDate
                totalSeats         = $seats
                price              = $price
                routeId            = $routeId
            }

            $res = Post-Api "/Admin/buses" $busPayload $adminToken
            if ($res) {
                $busCount++
                if ($busCount % 20 -eq 0) {
                    Write-Host "  ... $busCount buses created"
                }
            }
        }
    }
}

Write-Host ""
Write-Host "=== Seeding Complete ==="
Write-Host "  Routes    : $($routeIds.Count)"
Write-Host "  Operators : $($operatorList.Count)"
Write-Host "  Buses     : $busCount"
Write-Host "  Passengers: $($passengerList.Count) accounts (password: User@1234)"
Write-Host ""
Write-Host "Test accounts:"
Write-Host "  Admin    : admin@busbooking.com    / Admin@1234"
Write-Host "  Operator : tnexpress@operator.com  / Operator@1234"
Write-Host "  Passenger: rahul.kumar@test.com    / User@1234"
Write-Host ""
Write-Host "Try searching:"
Write-Host "  Chennai to Coimbatore"
Write-Host "  Chennai to Pondicherry"
Write-Host "  Madurai to Trichy"
