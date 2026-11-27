#!/usr/bin/env bash
# Kịch bản demo curl cho GV. Yêu cầu API đang chạy (mặc định http://localhost:5050) và có python3.
B=${1:-http://localhost:5050}
H='Content-Type: application/json'
code(){ curl -s -o /dev/null -w "%{http_code}" "$@"; }
field(){ python3 -c "import sys,json;print(json.load(sys.stdin)['$1'])"; }
login(){ curl -s -X POST $B/api/auth/login -H "$H" -d "{\"email\":\"$1\",\"password\":\"$2\"}"; }

echo "1) Không token -> GET /api/clubs:            $(code $B/api/clubs)"
echo "2) Login sai mật khẩu:                       $(code -X POST $B/api/auth/login -H "$H" -d '{"email":"admin@eduhub.local","password":"sai"}')"
ADMIN_RES=$(login admin@eduhub.local Admin@123);  echo "3) Login Admin: $(echo "$ADMIN_RES" | python3 -c "import sys,json;d=json.load(sys.stdin);print('expiresIn =',d['expiresIn'],'| tokenType =',d['tokenType'],'| có refreshToken =',bool(d['refreshToken']))")"
AT=$(echo "$ADMIN_RES" | field accessToken)
MT=$(login member@eduhub.local Member@123 | field accessToken)
OT=$(login organizer@eduhub.local Organizer@123 | field accessToken)
A="Authorization: Bearer $AT"; M="Authorization: Bearer $MT"; O="Authorization: Bearer $OT"

CLUB=$(curl -s $B/api/clubs -H "$A" | python3 -c "import sys,json;print(json.load(sys.stdin)[0]['id'])")
EVT=$(curl -s $B/api/events -H "$A" | python3 -c "import sys,json;print(json.load(sys.stdin)[0]['id'])")
echo "4) Member GET /api/clubs:                    $(code $B/api/clubs -H "$M")"
echo "5) Member DELETE club (sai role):            $(code -X DELETE $B/api/clubs/$CLUB -H "$M")"
echo "6) Member generate-qr (policy CanIssueQr):   $(code -X POST $B/api/events/$EVT/generate-qr -H "$M")"
echo "7) Organizer (claim qr.issue) generate-qr:   $(code -X POST $B/api/events/$EVT/generate-qr -H "$O")"
QR=$(curl -s -X POST $B/api/events/$EVT/generate-qr -H "$A")
echo "8) Admin generate-qr:                        $(echo "$QR" | python3 -c "import sys,json;d=json.load(sys.stdin);print('200 expiresAt =',d['expiresAt'])")"
PAYLOAD=$(echo "$QR" | field qrPayload)
echo "9) Member check-in khi CHƯA đăng ký:         $(code -X POST $B/api/events/$EVT/checkin -H "$M" -H "$H" -d "{\"qrPayload\":\"$PAYLOAD\"}")"
echo "10) Member đăng ký sự kiện:                  $(code -X POST $B/api/events/$EVT/register -H "$M")"
echo "11) Check-in với QR giả:                     $(code -X POST $B/api/events/$EVT/checkin -H "$M" -H "$H" -d '{"qrPayload":"abc.def"}')"
echo "12) Check-in hợp lệ:                         $(curl -s -w ' [%{http_code}]' -X POST $B/api/events/$EVT/checkin -H "$M" -H "$H" -d "{\"qrPayload\":\"$PAYLOAD\"}")"
echo "13) Check-in lần 2 (trùng):                  $(code -X POST $B/api/events/$EVT/checkin -H "$M" -H "$H" -d "{\"qrPayload\":\"$PAYLOAD\"}")"

RT1=$(echo "$ADMIN_RES" | field refreshToken)
R2=$(curl -s -X POST $B/api/auth/refresh -H "$H" -d "{\"refreshToken\":\"$RT1\"}")
echo "14) Refresh token lần 1 (đổi cặp mới):       $(echo "$R2" | python3 -c "import sys,json;d=json.load(sys.stdin);print('200, refreshToken mới =',bool(d['refreshToken']))")"
echo "15) Dùng lại refresh token cũ:               $(code -X POST $B/api/auth/refresh -H "$H" -d "{\"refreshToken\":\"$RT1\"}")"
echo "16) Refresh token mới cũng bị thu hồi:       $(code -X POST $B/api/auth/refresh -H "$H" -d "{\"refreshToken\":\"$(echo "$R2" | field refreshToken)\"}")"
echo "17) Đăng ký mật khẩu yếu (Identity từ chối): $(code -X POST $B/api/auth/register -H "$H" -d '{"email":"moi@eduhub.local","displayName":"Mới","password":"123"}')"
echo "18) Admin DELETE club:                       $(code -X DELETE $B/api/clubs/$CLUB -H "$A")"
