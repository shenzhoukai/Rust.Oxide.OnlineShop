# Rust.Oxide.OnlineShop
A backend plugin which allows players to login shop website and buy/sell Rust Items.

**OnlineShop** is a backend plugin which allows players to login shop website and buy/sell Rust Items.

It also can be used with third-party website programs, the most suitable one is [XiuZheZhiJia - SaaS   H5 rust server shop solution](https://rust.phellytech.com) which support for web shop with WeChatPay and Alipay for Chinese players and owners.

![XiuZheZhiJia - SaaS   H5 rust server shop solution](https://rust.phellytech.com/img/txtLogoZipped.png)

## Features
* Login Code (A 6-digit code)
  1. Find login code for target player.
  2. Find login code for the whole online player list.

* Item Search
  1. Find item from player's inventory.

* Player List Search
  1. Find the whole player list including online and offline.
  2. Find the whole online player list.
  3. Find the whole offile player list.

* Shop GUI
  1. Toggle on/off the display of shop button.
  2. Toggle on/off the display of help button.
  3. Toggle on/off the display of shop login form.

* Player Credit
  1. Search for target player's balance.
  2. Deposit target player's balance.
  3. Withdraw target player's balance.
  4. Reset target player's balance.

* Shopping
  1. Buy item for target player.
  2. Sell item for target player.

* Set Web API Parameters
  1. Set shop uniqid.
  2. Set admin login steamid.
  3. Set admin login pwd.

* Message Broadcast
  1. Send server a message for all players.
  2. Send player a message.

## Permissions
* `onlineshop.use` - allow player to click all buttons.
* `onlineshop.showshopbtn` - allow player to see shop button.
* `onlineshop.showhelpbtn` - allow player to see help button.

## Console Commands
* `fpfln all` - (Server Console Ony) List the login code of the whole online player list.
* `fpfln <steamid>` - (Server Console Ony) show the login code of target player.
* `fifb <steamid> <itemcode>` - (Server Console Ony) find item from target player's inventory.
* `gsul` - (Server Console Ony) List the whole player list including online and offline with their balance.
* `gsulonline` - (Server Console Ony) List the whole online player list including online and offline with their balance.
* `gsuloffline` - (Server Console Ony) List the whole offline player list including online and offline with their balance.
* `vshop` - (Client Console Ony) Show the shop login form，including shop website qrcode and current player's login code. The login code will refresh every 1 hour.
* `cshop` - (Client Console Ony) Hide the shop login form.
* `showshopbtn` - (Client Console Ony) Show the shop button.
* `hideshopbtn` - (Client Console Ony) Hide the shop button.
* `shopbalance <steamid>` - (Server Console Ony) Search for target player's balance.
* `shopdeposit <steamid> <pricevalue>` - (Server Console Ony) Deposit target player's balance.
* `shopwithdraw <steamid> <pricevalue>` - (Server Console Ony) Withdraw target player's balance.
* `shopsetbalance <steamid> <pricevalue>` - (Server Console Ony) Reset target player's balance.
* `shopbuy <steamid> <itemcode> <amount> <pricevalue>` - (Server Console Ony) Buy item for target player. Require target player is online and player has this enough balance and inventory place.
* `shopsell <steamid> <itemcode> <amount> <pricevalue>` - (Server Console Ony) Sell item for target player. Require target player is online and player has this item.
* `setshopuniqid <shopuniqId>` - (Server Console Ony) Set shop uniqid.
* `setadminloginsteamid <steamid>` - (Server Console Ony) Set admin login steamid.
* `setadminloginpwd <loginPwd>` - (Server Console Ony) Set admin login pwd.
* `sendservermsgbase64 <base64msg> <steamid>` - (Server Console Ony) Send server a message for all players. The message is base64 encoded and will be decoded when it is sent. The steamid is used to get chat avatar pic, it will show Rust icon if you use 0.
* `sendplayermsgbase64 <steamid> <base64msg>` - (Server Console Ony) Send player a message. The message is base64 encoded and will be decoded when it is sent.

## Default Configuration
```
{
  "Shop Config": {
    "ShopUniqID": "",
    "Admin Login SteamID": "",
    "Admin Login Pwd": ""
  },
  "GUI Form": {
    "Background color (RGBA format)": "0.20 0.20 0.20 0.99",
    "Button color (RGBA format)": "0.70 0.32 0.17 1.00",
    "Label color (RGBA format)": "1.96 2.55 0.00 1.00",
    "LoginNum Background color (RGBA format)": "0.20 0.20 0.20 0.00",
    "GUI Form Position": {
      "Anchors Min": "0.5 0",
      "Anchors Max": "0.5 0",
      "Offsets Min": "-198 362",
      "Offsets Max": "182 650",
      "LoginNum Anchors Min": "0.5 0",
      "LoginNum Anchors Max": "0.5 0",
      "LoginNum Offsets Min": "-198 362",
      "LoginNum Offsets Max": "182 410"
    }
  },
  "GUI Button": {
    "Image": "https://img.phellytech.com/rust/OnlineShop/shopMobile.png",
    "Background color (RGBA format)": "1 0.96 0.88 0.15",
    "GUI Button Position": {
      "Anchors Min": "0.5 0.0",
      "Anchors Max": "0.5 0.0",
      "Offsets Min": "249 18",
      "Offsets Max": "309 78"
    }
  },
  "GUI Help": {
    "Image": "https://img.phellytech.com/rust/OnlineShop/helpOpacity.png",
    "Background color (RGBA format)": "1 0.96 0.88 0.15",
    "GUI Button Position": {
      "Anchors Min": "0.5 0.0",
      "Anchors Max": "0.5 0.0",
      "Offsets Min": "313 18",
      "Offsets Max": "343 48"
    }
  }
}
```

## Default Translation
en:
```
{
  "Shop Form Title": "Online Shop",
  "Shop Form Close": "Close",
  "Shop Form Please Scan Qrcode": "Please Scan Qrcode to Access Shop",
  "Shop Form My Login Code": "My Login Code: ",
  "Shop Form Please Dont Leak": "(Please Dont Leak)",
  "Your Balance Deposit Point": "Your Balance Deposit {0} Point",
  "Your Balance Withdraw Point": "Your Balance Withdraw {0} Point",
  "Your Balance Reset Point": "Your Balance Reset to {0} Point"
}
```

zh-CN:
```
{
  "Shop Form Title": "微信H5在线商店",
  "Shop Form Close": "关闭",
  "Shop Form Please Scan Qrcode": "扫描下方二维码，进入在线商店",
  "Shop Form My Login Code": "我的令牌码：",
  "Shop Form Please Dont Leak": "（请勿泄露）",
  "Your Balance Deposit Point": "您的钱包余额已增加 {0} 点",
  "Your Balance Withdraw Point": "您的钱包余额已减少 {0} 点",
  "Your Balance Reset Point": "您的钱包余额已重置为 {0} 点"
}
```
