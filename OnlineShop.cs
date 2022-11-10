using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("OnlineShop", "CainSZK", "22.11.11")]
    [Description("A backend plugin which allows players to login shop website and buy/sell Rust Items.")]
    class OnlineShop : RustPlugin
    {
        [PluginReference] Plugin Economics;

        private const string GUIBtnPanelName = "OnlineShopBtnUI";
        private const string GUIBtnHelpName = "OnlineShopHelpUI";
        private const string GUIFormPanelName = "OnlineShopFormUI";
        private const string GUILoginNumPanelName = "OnlineShopLoginNumUI";

        private const string UsagePermission = "onlineshop.use";
        private const string ShowShopBtnPermission = "onlineshop.showshopbtn";
        private const string ShowHelpBtnPermission = "onlineshop.showhelpbtn";

        private const string CmdShowGuiBtn = "showshopbtn";
        private const string CmdHideGuiBtn = "hideshopbtn";
        private const string CmdViewMyShop = "vshop";
        private const string CmdCloseMyShop = "cshop";
        private const string CmdShowHelp = "chat.teamsay /info";
		
        private const string CmdFindItemFromBag = "fifb";
        private const string CmdFindPlayerFromLoginNum = "fpfln";
        private const string CmdSellItems = "shopsell";
        private const string CmdBuyItems = "shopbuy";
        private const string CmdBalanceDeposit = "shopdeposit";
        private const string CmdBalanceWithdraw = "shopwithdraw";
        private const string CmdSetBalance = "shopsetbalance";
        private const string CmdCheckBalance = "shopbalance";

        private const string CmdGetServerUserList = "gsul";
        private const string CmdGetServerUserListOnline = "gsulonline";
        private const string CmdGetServerUserListOffline = "gsuloffline";
        private const string CmdSetShopUniqID = "setshopuniqid";
        private const string CmdSetAdminLoginSteamID = "setadminloginsteamid";
        private const string CmdSetAdminLoginPwd = "setadminloginpwd";

        private const string CmdSendServerMsgBase64 = "sendservermsgbase64";
        private const string CmdSendPlayerMsgBase64 = "sendplayermsgbase64";

        private static OnlineShop _instance;
        
        private string _cachedBtnUI;
        private string _cachedHelpUI;
        private string _cachedFormUI;
        private string _cachedLoginNumUI;
        #region Config
        private Configuration _config;
        private StoredData _storedData;
        private class Configuration
        {
            [JsonProperty("Shop Config")]
            public ShopConfig ShopCfg = new ShopConfig();
            public class ShopConfig
            {
                [JsonProperty(PropertyName = "ShopUniqID")]
                public string ShopUniqID = "";

                [JsonProperty(PropertyName = "Admin Login SteamID")]
                public string AdminLoginSteamID = "";

                [JsonProperty(PropertyName = "Admin Login Pwd")]
                public string AdminLoginPwd = "";
            }
            [JsonProperty("GUI Form")]
            public GUIForm GUIFrm = new GUIForm();
            public class GUIForm
            {
                [JsonProperty(PropertyName = "Background color (RGBA format)")]
                public string Color = "0.20 0.20 0.20 0.99";

                [JsonProperty(PropertyName = "Button color (RGBA format)")]
                public string BtnColor = "0.70 0.32 0.17 1.00";

                [JsonProperty(PropertyName = "Label color (RGBA format)")]
                public string LbColor = "1.96 2.55 0.00 1.00";

                [JsonProperty(PropertyName = "LoginNum Background color (RGBA format)")]
                public string LoginNumBgColor = "0.20 0.20 0.20 0.00";

                [JsonProperty(PropertyName = "GUI Form Position")]
                public Position GUIFormPosition = new Position();
                public class Position
                {
                    [JsonProperty(PropertyName = "Anchors Min")]
                    public string AnchorsMin = "0.5 0";

                    [JsonProperty(PropertyName = "Anchors Max")]
                    public string AnchorsMax = "0.5 0";

                    [JsonProperty(PropertyName = "Offsets Min")]
                    public string OffsetsMin = "-198 362";

                    [JsonProperty(PropertyName = "Offsets Max")]
                    public string OffsetsMax = "182 650";

                    [JsonProperty(PropertyName = "LoginNum Anchors Min")]
                    public string LoginNumAnchorsMin = "0.5 0";

                    [JsonProperty(PropertyName = "LoginNum Anchors Max")]
                    public string LoginNumAnchorsMax = "0.5 0";

                    [JsonProperty(PropertyName = "LoginNum Offsets Min")]
                    public string LoginNumOffsetsMin = "-198 362";

                    [JsonProperty(PropertyName = "LoginNum Offsets Max")]
                    public string LoginNumOffsetsMax = "182 410";
                }
            }
            [JsonProperty("GUI Button")]
            public GUIButton GUIBtn = new GUIButton();
            public class GUIButton
            {
                [JsonProperty(PropertyName = "Image")]
                public string Image = "https://img.phellytech.com/rust/OnlineShop/shopMobile.png";

                [JsonProperty(PropertyName = "Background color (RGBA format)")]
                public string Color = "1 0.96 0.88 0.15";

                [JsonProperty(PropertyName = "GUI Button Position")]
                public Position GUIButtonPosition = new Position();
                public class Position
                {
                    [JsonProperty(PropertyName = "Anchors Min")]
                    public string AnchorsMin = "0.5 0.0";

                    [JsonProperty(PropertyName = "Anchors Max")]
                    public string AnchorsMax = "0.5 0.0";

                    [JsonProperty(PropertyName = "Offsets Min")]
                    public string OffsetsMin = "249 18";

                    [JsonProperty(PropertyName = "Offsets Max")]
                    public string OffsetsMax = "309 78";
                }
            }
            [JsonProperty("GUI Help")]
            public GUIHelp GUIHlp = new GUIHelp();
            public class GUIHelp
            {
                [JsonProperty(PropertyName = "Image")]
                public string Image = "https://img.phellytech.com/rust/OnlineShop/helpOpacity.png";

                [JsonProperty(PropertyName = "Background color (RGBA format)")]
                public string Color = "1 0.96 0.88 0.15";

                [JsonProperty(PropertyName = "GUI Button Position")]
                public Position GUIButtonPosition = new Position();
                public class Position
                {
                    [JsonProperty(PropertyName = "Anchors Min")]
                    public string AnchorsMin = "0.5 0.0";

                    [JsonProperty(PropertyName = "Anchors Max")]
                    public string AnchorsMax = "0.5 0.0";

                    [JsonProperty(PropertyName = "Offsets Min")]
                    public string OffsetsMin = "313 18";

                    [JsonProperty(PropertyName = "Offsets Max")]
                    public string OffsetsMax = "343 48";
                }
            }
        }
        private class StoredData
        {
            public static StoredData Load()
            {
                var data = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(_instance.Name);
                if (data == null)
                {
                    _instance.PrintWarning($"Data file {_instance.Name}.json is invalid. Creating new data file.");
                    data = new StoredData();
                    data.Save();
                }
                return data;
            }

            [JsonProperty("PluginInstallDateTime")]
            public string PluginInstallDateTime = string.Empty;

            public void Save() =>
                Interface.Oxide.DataFileSystem.WriteObject(_instance.Name, this);
        }
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Shop Form Title"] = "Online Shop",
                ["Shop Form Close"] = "Close",
                ["Shop Form Please Scan Qrcode"] = "Please Scan Qrcode to Access Shop",
                ["Shop Form My Login Code"] = "My Login Code: ",
                ["Shop Form Please Dont Leak"] = "(Please Dont Leak)",
                ["Your Balance Deposit Point"] = "Your Balance Deposit {0} Point",
                ["Your Balance Withdraw Point"] = "Your Balance Withdraw {0} Point",
                ["Your Balance Reset Point"] = "Your Balance Reset to {0} Point",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Shop Form Title"] = "微信H5在线商店",
                ["Shop Form Close"] = "关闭",
                ["Shop Form Please Scan Qrcode"] = "扫描下方二维码，进入在线商店",
                ["Shop Form My Login Code"] = "我的令牌码：",
                ["Shop Form Please Dont Leak"] = "（请勿泄露）",
                ["Your Balance Deposit Point"] = "您的钱包余额已增加 {0} 点",
                ["Your Balance Withdraw Point"] = "您的钱包余额已减少 {0} 点",
                ["Your Balance Reset Point"] = "您的钱包余额已重置为 {0} 点",
            }, this, "zh-CN");
        }
        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();
            SaveConfig();
        }
        protected override void SaveConfig() => Config.WriteObject(_config);
        protected override void LoadDefaultConfig()
        {
            _config = new Configuration{};
            SaveConfig();
        }
        #endregion
        private void RegPlugPms()
        {
            permission.RegisterPermission(UsagePermission, this);
            permission.RegisterPermission(ShowShopBtnPermission, this);
            permission.RegisterPermission(ShowHelpBtnPermission, this);
            _storedData = StoredData.Load();
            if(string.IsNullOrEmpty(_storedData.PluginInstallDateTime))
            {
                permission.GrantGroupPermission("default", UsagePermission, this);
                permission.GrantGroupPermission("default", ShowShopBtnPermission, this);
                permission.GrantGroupPermission("default", ShowHelpBtnPermission, this);
                _storedData.PluginInstallDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _storedData.Save();
            }
        }
        private void Init()
        {
            Puts("Plugin OnlineShop Debug Start");
            _instance = this;
            RegPlugPms();
            Unsubscribe(nameof(OnPlayerSleep));
            Unsubscribe(nameof(OnPlayerSleepEnded));
            Puts("Plugin OnlineShop Debug End");
        }
        private void ServerMsg(string strMsg, ulong chatID)
        {
            IEnumerable<BasePlayer> userOnline = GetOnlineUserList();
            foreach(BasePlayer player in userOnline)
            {
                if(player.UserIDString.Length == 17)
                {
                    _instance.Player.Message(player, strMsg, chatID);
                }
            }
        }
        private void PlayerMsg(BasePlayer player,string strMsg)
        {
            if(!string.IsNullOrEmpty(strMsg))
            {
                IPlayer p = player.IPlayer;
                p.Reply(strMsg);
            }
        }
        private void PlayerMsgBySteamID(string strSteamID,string strMsg)
        {
            IEnumerable<BasePlayer> userOnline = GetOnlineUserList();
            foreach(BasePlayer player in userOnline)
            {
                if(player.UserIDString == strSteamID)
                {
                    PlayerMsg(player,strMsg);
                    break;
                }
            }
        }
        private void CreateBtnGUI(BasePlayer player)
        {
            if (player == null || player.IsNpc || !player.IsAlive() || player.IsSleeping())
                return;
            
            if (!permission.UserHasPermission(player.UserIDString, ShowShopBtnPermission))
                return;
            CuiHelper.DestroyUi(player, GUIBtnPanelName);
            if (_cachedBtnUI == null)
            {
                var elements = new CuiElementContainer();
                var OnlineShopBtnUIPanel = elements.Add(new CuiPanel
                {
                    Image = { Color = _instance._config.GUIBtn.Color },
                    RectTransform = {
                        AnchorMin = _config.GUIBtn.GUIButtonPosition.AnchorsMin,
                        AnchorMax = _config.GUIBtn.GUIButtonPosition.AnchorsMax,
                        OffsetMin = _config.GUIBtn.GUIButtonPosition.OffsetsMin,
                        OffsetMax = _config.GUIBtn.GUIButtonPosition.OffsetsMax
                    },
                    CursorEnabled = false
                }, "Overlay", GUIBtnPanelName);

                elements.Add(new CuiElement
                {
                    Parent = GUIBtnPanelName,
                    Components = {
                        new CuiRawImageComponent { Url = _instance._config.GUIBtn.Image },
                        new CuiRectTransformComponent { AnchorMin = "0 0", AnchorMax = "1 1" }
                    }
                });

                elements.Add(new CuiButton
                {
                    Button = { Command = CmdViewMyShop, Color = "0 0 0 0" },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    Text = { Text = "" }
                }, OnlineShopBtnUIPanel);
                _cachedBtnUI = CuiHelper.ToJson(elements);
            }
            CuiHelper.AddUi(player, _cachedBtnUI);
        }
        private void CreateHelpGUI(BasePlayer player)
        {
            if (player == null || player.IsNpc || !player.IsAlive() || player.IsSleeping())
                return;
            if (!permission.UserHasPermission(player.UserIDString, ShowHelpBtnPermission))
                return;
            CuiHelper.DestroyUi(player, GUIBtnHelpName);
            if (_cachedHelpUI == null)
            {
                var elements = new CuiElementContainer();
                var OnlineShopHelpUIPanel = elements.Add(new CuiPanel
                {
                    Image = { Color = _instance._config.GUIHlp.Color },
                    RectTransform = {
                        AnchorMin = _config.GUIHlp.GUIButtonPosition.AnchorsMin,
                        AnchorMax = _config.GUIHlp.GUIButtonPosition.AnchorsMax,
                        OffsetMin = _config.GUIHlp.GUIButtonPosition.OffsetsMin,
                        OffsetMax = _config.GUIHlp.GUIButtonPosition.OffsetsMax
                    },
                    CursorEnabled = false
                }, "Overlay", GUIBtnHelpName);

                elements.Add(new CuiElement
                {
                    Parent = GUIBtnHelpName,
                    Components = {
                        new CuiRawImageComponent { Url = _instance._config.GUIHlp.Image },
                        new CuiRectTransformComponent { AnchorMin = "0 0", AnchorMax = "1 1" }
                    }
                });

                elements.Add(new CuiButton
                {
                    Button = { Command = CmdShowHelp, Color = "0 0 0 0" },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    Text = { Text = "" }
                }, OnlineShopHelpUIPanel);
                _cachedHelpUI = CuiHelper.ToJson(elements);
            }
            CuiHelper.AddUi(player, _cachedHelpUI);
        }
        private void CreateFormGUI(BasePlayer player)
        {
            if (player == null || player.IsNpc || !player.IsAlive() || player.IsSleeping())
                return;
            if (!permission.UserHasPermission(player.UserIDString, ShowShopBtnPermission))
                return;
            CuiHelper.DestroyUi(player, GUIFormPanelName);
            if (_cachedFormUI == null)
            {
                var elements = new CuiElementContainer();
                var OnlineShopFormUIPanel = elements.Add(new CuiPanel
                {
                    Image = { Color = _instance._config.GUIFrm.Color },
                    RectTransform = {
                        AnchorMin = _config.GUIFrm.GUIFormPosition.AnchorsMin,
                        AnchorMax = _config.GUIFrm.GUIFormPosition.AnchorsMax,
                        OffsetMin = _config.GUIFrm.GUIFormPosition.OffsetsMin,
                        OffsetMax = _config.GUIFrm.GUIFormPosition.OffsetsMax
                    },
                    CursorEnabled = false
                }, "Overlay", GUIFormPanelName);

                elements.Add(new CuiElement
                {
                    Parent = GUIFormPanelName,
                    Components = {
                        new CuiRawImageComponent { Url = EncodeShopQrcodeUrl(_config.ShopCfg.ShopUniqID) },
                        new CuiRectTransformComponent 
                        {
                            AnchorMin = "0.5 0.5",
                            AnchorMax = "0.5 0.5",
                            OffsetMin = "-70 -80",
                            OffsetMax = "70 60"
                        }
                    }
                });
                
                elements.Add(new CuiLabel
                {
                    RectTransform = 
                    { 
                        AnchorMin = "0 1",
                        AnchorMax = "0 1",
                        OffsetMin = "5 -30",
                        OffsetMax = "180 0"
                    },
                    Text = 
                    {
                        Text = "<color=#C4FF00>" + lang.GetMessage("Shop Form Title", this, player.IPlayer.Id) + "</color>",
                        FontSize = 18,
                        Align = TextAnchor.MiddleLeft
                    }
                }, OnlineShopFormUIPanel);

                elements.Add(new CuiLabel
                {
                    RectTransform = 
                    { 
                        AnchorMin = "0 1",
                        AnchorMax = "0 1",
                        OffsetMin = "0 -110",
                        OffsetMax = "360 0"
                    },
                    Text = 
                    {
                        Text = lang.GetMessage("Shop Form Please Scan Qrcode", this, player.IPlayer.Id),
                        FontSize = 18,
                        Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 0.5"
                    }
                }, OnlineShopFormUIPanel);

                elements.Add(new CuiButton
                {
                    Button = { Command = CmdCloseMyShop, Color = _config.GUIFrm.BtnColor },
                    RectTransform = 
                    { 
                        AnchorMin = "1 1",
                        AnchorMax = "1 1",
                        OffsetMin = "-60 -30",
                        OffsetMax = "0 0"
                    },
                    Text = 
                    {
                        Text = lang.GetMessage("Shop Form Close", this, player.IPlayer.Id),
                        FontSize = 18,
                        Align = TextAnchor.MiddleCenter
                    }
                }, OnlineShopFormUIPanel);
                _cachedFormUI = CuiHelper.ToJson(elements);
            }
            CuiHelper.AddUi(player, _cachedFormUI);
        }
        private void CreateLoginNumGUI(BasePlayer player)
        {
            if (player == null || player.IsNpc || !player.IsAlive() || player.IsSleeping())
                return;
            if (!permission.UserHasPermission(player.UserIDString, ShowShopBtnPermission))
                return;
            CuiHelper.DestroyUi(player, GUILoginNumPanelName);
            if (1 == 1)
            {
                var elements = new CuiElementContainer();
                var OnlineShopLoginNumUIPanel = elements.Add(new CuiPanel
                {
                    Image = { Color = _instance._config.GUIFrm.LoginNumBgColor },
                    RectTransform = {
                        AnchorMin = _config.GUIFrm.GUIFormPosition.LoginNumAnchorsMin,
                        AnchorMax = _config.GUIFrm.GUIFormPosition.LoginNumAnchorsMax,
                        OffsetMin = _config.GUIFrm.GUIFormPosition.LoginNumOffsetsMin,
                        OffsetMax = _config.GUIFrm.GUIFormPosition.LoginNumOffsetsMax
                    },
                    CursorEnabled = false
                }, "Overlay", GUILoginNumPanelName);
                
                elements.Add(new CuiLabel
                {
                    RectTransform = 
                    { 
                        AnchorMin = "0 0",
                        AnchorMax = "0 0",
                        OffsetMin = "0 0",
                        OffsetMax = "360 65"
                    },
                    Text = 
                    {
                        Text = lang.GetMessage("Shop Form My Login Code", this, player.IPlayer.Id) + "<color=orange>" + EncodeLoginNum(player.UserIDString) + "</color>" + lang.GetMessage("Shop Form Please Dont Leak", this, player.IPlayer.Id),
                        FontSize = 18,
                        Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 0.5"
                    }
                }, OnlineShopLoginNumUIPanel);

                _cachedLoginNumUI = CuiHelper.ToJson(elements);
            }
            CuiHelper.AddUi(player, _cachedLoginNumUI);
        }
        private void DestroyBtnGUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, GUIBtnPanelName);
        }
        private void DestroyHelpGUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, GUIBtnHelpName);
        }
        private void DestroyFormGUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, GUIFormPanelName);
        }
        private void DestroyLoginNumGUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, GUILoginNumPanelName);
        }
        private void DestroyAllGUI(BasePlayer player)
        {
            if(player == null)
	            return;
            DestroyBtnGUI(player);
			DestroyHelpGUI(player);
            DestroyFormGUI(player);
            DestroyLoginNumGUI(player);
        }
        #region Hooks
        private void OnServerInitialized()
        {
            var players = _instance.covalence.Players;
            foreach (var player in players.Connected)
            {
                DestroyBtnGUI(player.Object as BasePlayer);
				DestroyHelpGUI(player.Object as BasePlayer);
                DestroyFormGUI(player.Object as BasePlayer);
                DestroyLoginNumGUI(player.Object as BasePlayer);
                CreateBtnGUI(player.Object as BasePlayer);
				CreateHelpGUI(player.Object as BasePlayer);
            }
            Subscribe(nameof(OnPlayerSleep));
            Subscribe(nameof(OnPlayerSleepEnded));
        }
        private void OnGroupPermissionGranted(string group, string perm)
        {
            if (perm.Equals(ShowShopBtnPermission))
            {
                foreach (IPlayer player in covalence.Players.Connected.Where(p => permission.UserHasGroup(p.Id, group)))
                {
                    CreateBtnGUI(player.Object as BasePlayer);
                }
            }	
            if (perm.Equals(ShowHelpBtnPermission))
            {
                foreach (IPlayer player in covalence.Players.Connected.Where(p => permission.UserHasGroup(p.Id, group)))
                {
                    CreateHelpGUI(player.Object as BasePlayer);
                }
            }
        }
        private void OnGroupPermissionRevoked(string group, string perm)
        {
            if (perm.Equals(ShowShopBtnPermission))
            {
                foreach (IPlayer player in covalence.Players.Connected.Where(p => permission.UserHasGroup(p.Id, group)))
                {
                    if (!permission.UserHasPermission(player.Id, ShowShopBtnPermission))
                    {
                        DestroyBtnGUI(player.Object as BasePlayer);
                    }
                }
            }
            if (perm.Equals(ShowHelpBtnPermission))
            {
                foreach (IPlayer player in covalence.Players.Connected.Where(p => permission.UserHasGroup(p.Id, group)))
                {
                    if (!permission.UserHasPermission(player.Id, ShowHelpBtnPermission))
                    {
                        DestroyHelpGUI(player.Object as BasePlayer);
                    }
                }
            }
        }
        private void OnUserPermissionGranted(string userId, string perm)
        {
            if (perm.Equals(ShowShopBtnPermission))
                CreateBtnGUI(_instance.covalence.Players.FindPlayerById(userId).Object as BasePlayer);
            if (perm.Equals(ShowHelpBtnPermission))
                CreateHelpGUI(_instance.covalence.Players.FindPlayerById(userId).Object as BasePlayer);
        }
        private void OnUserPermissionRevoked(string userId, string perm)
        {
            if (perm.Equals(ShowShopBtnPermission) && !permission.UserHasPermission(userId, ShowShopBtnPermission))
                DestroyBtnGUI(_instance.covalence.Players.FindPlayerById(userId).Object as BasePlayer);
            if (perm.Equals(ShowHelpBtnPermission) && !permission.UserHasPermission(userId, ShowHelpBtnPermission))
                DestroyHelpGUI(_instance.covalence.Players.FindPlayerById(userId).Object as BasePlayer);
        }
        private void CreateAllGUI(BasePlayer player)
        {
            CreateBtnGUI(player);
            CreateHelpGUI(player);
        }
        private void OnPlayerConnected(BasePlayer player) => CreateAllGUI(player);
        private void OnPlayerRespawned(BasePlayer player) => CreateAllGUI(player);
        private void OnPlayerSleepEnded(BasePlayer player) => CreateAllGUI(player);
        private void OnPlayerSleep(BasePlayer player) => DestroyAllGUI(player);
        private void OnEntityDeath(BasePlayer player, HitInfo info) => OnEntityKill(player);
        private void OnEntityKill(BasePlayer player)
        {
            if (player.IsNpc)
                return;
            DestroyAllGUI(player);
        }
        #endregion
        #region Commands
        [ConsoleCommand(CmdSendPlayerMsgBase64)]
        private void SendPlayerMsgBase64(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //sendplayermsgbase64 76560000000000000 5o+S5Lu25rWL6K+V
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 2)
            {
                //Invalid Parameter Numbers
                strMsg = CmdSendPlayerMsgBase64 + ",-,-";
                Puts(strMsg);
                return;
            }
            var strSteamID = args[0];
            var strSendMsg = DecodeBase64(args[1]);
            PlayerMsgBySteamID(strSteamID,strSendMsg);
            strMsg = string.Format(CmdSendPlayerMsgBase64 + ",{0},{1}",strSteamID,strSendMsg);
            Puts(strMsg);
        }
        [ConsoleCommand(CmdSendServerMsgBase64)]
        private void SendServerMsgBase64(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //sendservermsgbase64 5o+S5Lu25rWL6K+V 0
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 2)
            {
                //Invalid Parameter Numbers
                strMsg = CmdSendServerMsgBase64 + ",-,-";
                Puts(strMsg);
                return;
            }
            var chatID = ulong.Parse(args[1]);
            var strSendMsg = DecodeBase64(args[0]);
            ServerMsg(strSendMsg,chatID);
            strMsg = string.Format(CmdSendServerMsgBase64 + ",{0},{1}",strSendMsg,chatID);
            Puts(strMsg);
        }
        [ConsoleCommand(CmdSetShopUniqID)]
        private void SetShopUniqID(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //setshopuniqid ABCDEF
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 1)
            {
                //Invalid Parameter Numbers
                strMsg = CmdSetShopUniqID + ",-";
                Puts(strMsg);
                return;
            }
            var strShopUniqID = args[0];
            _config.ShopCfg.ShopUniqID = strShopUniqID;
            SaveConfig();
            strMsg = string.Format(CmdSetShopUniqID + ",{0}",strShopUniqID);
            Puts(strMsg);
        }
        [ConsoleCommand(CmdSetAdminLoginSteamID)]
        private void SetAdminLoginSteamID(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //setadminloginsteamid 76560000000000000
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 1)
            {
                //Invalid Parameter Numbers
                strMsg = CmdSetAdminLoginSteamID + ",-";
                Puts(strMsg);
                return;
            }
            var strAdminLoginSteamID = args[0];
            _config.ShopCfg.AdminLoginSteamID = strAdminLoginSteamID;
            SaveConfig();
            strMsg = string.Format(CmdSetAdminLoginSteamID + ",{0}",strAdminLoginSteamID);
            Puts(strMsg);
        }
        [ConsoleCommand(CmdSetAdminLoginPwd)]
        private void SetAdminLoginPwd(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //setadminloginpwd 76560000000000000
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 1)
            {
                //Invalid Parameter Numbers
                strMsg = CmdSetAdminLoginPwd + ",-";
                Puts(strMsg);
                return;
            }
            var strAdminLoginPwd = args[0];
            _config.ShopCfg.AdminLoginPwd = strAdminLoginPwd;
            SaveConfig();
            strMsg = string.Format(CmdSetAdminLoginPwd + ",{0}",strAdminLoginPwd);
            Puts(strMsg);
        }
        [ConsoleCommand(CmdCheckBalance)]
        private void CheckBalance(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //shopbalance 76560000000000000
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 1)
            {
                //Invalid Parameter Numbers
                strMsg = CmdCheckBalance+",-,-";
                Puts(strMsg);
                return;
            }
            var strSteamID = args[0];
            var strBalance = Balance(strSteamID).ToString();
            strMsg = string.Format(CmdCheckBalance+",{0},{1}",strSteamID,strBalance);
            Puts(strMsg);
        }
        [ConsoleCommand(CmdBalanceDeposit)]
        private void BalanceDeposit(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //shopdeposit 76560000000000000 1
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 2)
            {
                //Invalid Parameter Numbers
                strMsg = CmdBalanceDeposit+",-,failed";
                Puts(strMsg);
                return;
            }
            var strSteamID = args[0];
            var strPointAmount = args[1];
            if(Deposit(strSteamID,double.Parse(strPointAmount)))
            {
                strMsg = string.Format(CmdBalanceDeposit+",{0},{1}",strSteamID,"success");
                PlayerMsgBySteamID(strSteamID,string.Format(lang.GetMessage("Your Balance Deposit Point", this, player.IPlayer.Id),strPointAmount));
            }
            else
            {
                strMsg = string.Format(CmdBalanceDeposit+",{0},{1}",strSteamID,"failed");
            }
            Puts(strMsg);
        }
        [ConsoleCommand(CmdBalanceWithdraw)]
        private void BalanceWithdraw(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //shopwithdraw 76560000000000000 1
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 2)
            {
                //Invalid Parameter Numbers
                strMsg = CmdBalanceWithdraw+",-,failed";
                Puts(strMsg);
                return;
            }
            var strSteamID = args[0];
            var strPointAmount = args[1];
            if(Withdraw(strSteamID,double.Parse(strPointAmount)))
            {
                strMsg = string.Format(CmdBalanceWithdraw+",{0},{1}",strSteamID,"success");
                PlayerMsgBySteamID(strSteamID,string.Format(lang.GetMessage("Your Balance Withdraw Point", this, player.IPlayer.Id),strPointAmount));
            }
            else
            {
                strMsg = string.Format(CmdBalanceWithdraw+",{0},{1}",strSteamID,"failed");
            }
            Puts(strMsg);
        }
        [ConsoleCommand(CmdSetBalance)]
        private void SetBalance(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //shopsetbalance 76560000000000000 1
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 2)
            {
                //Invalid Parameter Numbers
                strMsg = CmdSetBalance+",-,failed";
                Puts(strMsg);
                return;
            }
            var strSteamID = args[0];
            var strPointAmount = args[1];
            if(SetBalance(strSteamID,double.Parse(strPointAmount)))
            {
                strMsg = string.Format(CmdSetBalance+",{0},{1}",strSteamID,"success");
                PlayerMsgBySteamID(strSteamID,string.Format(lang.GetMessage("Your Balance Reset Point", this, player.IPlayer.Id),strPointAmount));
            }
            else
            {
                strMsg = string.Format(CmdSetBalance+",{0},{1}",strSteamID,"failed");
            }
            Puts(strMsg);
        }
        [ConsoleCommand(CmdShowGuiBtn)]
        private void ShowGuiBtn(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //showshopbtn
            //---- Command Format ----//
            BasePlayer player = arg.Player();
            IPlayer p = player.IPlayer;
            if (!p.HasPermission(UsagePermission))
                return;
            if(p.IsServer)
            {
                //Is Server Side Action
                return;
            }
            //Not Server Side Action
            CreateBtnGUI(player);
        }
        [ConsoleCommand(CmdHideGuiBtn)]
        private void HideGuiBtn(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //hideshopbtn
            //---- Command Format ----//
            BasePlayer player = arg.Player();
            IPlayer p = player.IPlayer;
            if (!p.HasPermission(UsagePermission))
                return;
            if(p.IsServer)
            {
                //Is Server Side Action
                return;
            }
            //Not Server Side Action
            DestroyBtnGUI(player);
        }
        [ConsoleCommand(CmdViewMyShop)]
        private void ViewMyShop(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //vshop
            //---- Command Format ----//
            BasePlayer player = arg.Player();
            IPlayer p = player.IPlayer;
            if (!p.HasPermission(UsagePermission))
                return;
            if(p.IsServer)
            {
                //Is Server Side Action
                return;
            }
            //Not Server Side Action
            CreateFormGUI(player);
            CreateLoginNumGUI(player);
        }
        [ConsoleCommand(CmdCloseMyShop)]
        private void CloseMyShop(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //cshop
            //---- Command Format ----//
            BasePlayer player = arg.Player();
            IPlayer p = player.IPlayer;
            if(p.IsServer)
            {
                //Is Server Side Action
                return;
            }
            //Not Server Side Action
            DestroyFormGUI(player);
            DestroyLoginNumGUI(player);
        }
        [ConsoleCommand(CmdFindItemFromBag)]
        private void FindItemFromBag(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //fifb 76560000000000000 itemCode
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 2)
            {
                //Invalid Parameter Numbers
                strMsg = CmdFindItemFromBag+",-,-,-";
                Puts(strMsg);
                return;
            }
            var strSteamID = args[0];
            var strItemName = args[1];
            if(args[0].Length != 17)
            {
                //Invalid SteamID
                strMsg = string.Format(CmdFindItemFromBag+"b,{0},{1},-",strSteamID,strItemName);
            }
            if(string.IsNullOrEmpty(strMsg))
            {
                //Valid Parameter, Start Query
                IPlayer target = _instance.covalence.Players.FindPlayerById(strSteamID);
                if(target == null)
                {
                    //Cant Find Player
                    strMsg = string.Format(CmdFindItemFromBag+",{0},{1},-",strSteamID,strItemName);
                }
                else
                {
                    var amount = GetAmount(target, strItemName);
                    strMsg = string.Format(CmdFindItemFromBag+",{0},{1},{2}",strSteamID,strItemName,amount);
                }
            }
            Puts(strMsg);
        }
        [ConsoleCommand(CmdFindPlayerFromLoginNum)]
        private void FindPlayerFromLoginNum(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //fpfln 000000
            //---- Command Format ----//
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            var blFound = false;
            var strSteamID = string.Empty;
            if(args.Length != 1)
            {
                //Invalid Parameter Numbers
                return;
            }
            var players = _instance.covalence.Players;
            var strLoginNum = args[0];
            if(strLoginNum == "all")
            {
                foreach (var iplayer in players.Connected)
                {
                    var strPlayerLoginNum = EncodeLoginNum(iplayer.Id);
                    Puts(string.Format(CmdFindPlayerFromLoginNum+",{0},{1},{2}",iplayer.Id,iplayer.Name,strPlayerLoginNum));
                }
            }
            else
            {
                if(strLoginNum == _config.ShopCfg.AdminLoginPwd)
                {
                    Puts(string.Format(CmdFindPlayerFromLoginNum+",{0},{1}",strLoginNum,_config.ShopCfg.AdminLoginSteamID));
                }
                else
                {
                    foreach (var iplayer in players.Connected)
                    {
                        if(!blFound)
                        {
                            if(EncodeLoginNum(iplayer.Id) == strLoginNum)
                            {
                                strSteamID = iplayer.Id;
                                blFound = true;
                            }
                        }
                    }
                    if(blFound)
                    {
                        Puts(string.Format(CmdFindPlayerFromLoginNum+",{0},{1}",strLoginNum,strSteamID));
                    }
                    else
                    {
                        Puts(string.Format(CmdFindPlayerFromLoginNum+",{0},-",strLoginNum));
                    }
                }
            }
        }
        [ConsoleCommand(CmdBuyItems)]
        private void BuyItems(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //shopbuy 76560000000000000 itemCode num price
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 4)
            {
                //Invalid Parameter Numbers
                strMsg = CmdBuyItems+",-,-,-,-,-";
                Puts(strMsg);
                return;
            }
            var strSteamID = args[0];
            var strItemCode = args[1];
            var amount = int.Parse(args[2]);
            var buyPrice = args[3];
            IPlayer target = _instance.covalence.Players.FindPlayerById(strSteamID);
            if(target == null)
            {
                strMsg = string.Format(CmdBuyItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,buyPrice,"offline");
                Puts(strMsg);
                return;
            }
            if(!target.IsConnected)
            {
                strMsg = string.Format(CmdBuyItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,buyPrice,"offline");
                Puts(strMsg);
                return;
            }
            var bplayer = target.Object as BasePlayer;
            if(Balance(strSteamID) < double.Parse(buyPrice))
            {
                strMsg = string.Format(CmdBuyItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,buyPrice,"invalidbalance");
                Puts(strMsg);
                return;
            }
            if(bplayer.inventory.containerMain.IsFull())
            {
                var playerHasAmount = GetAmount(target, strItemCode);
                if(playerHasAmount == 0)
                {
                    strMsg = string.Format(CmdBuyItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,buyPrice,"isfull");
                    Puts(strMsg);
                }
                else
                {
                    AddItem(bplayer.inventory.containerMain, strItemCode, amount);
                    bplayer.inventory.SendUpdatedInventory(PlayerInventory.Type.Main, bplayer.inventory.containerMain);
                    if(Withdraw(strSteamID,double.Parse(buyPrice)))
                    {
                        strMsg = string.Format(CmdBuyItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,buyPrice,"success");
                    }
                    else
                    {
                        strMsg = string.Format(CmdBuyItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,buyPrice,"failed");
                    }
                    Puts(strMsg);
                }
            }
            else
            {
                AddItem(bplayer.inventory.containerMain, strItemCode, amount);
                bplayer.inventory.SendUpdatedInventory(PlayerInventory.Type.Main, bplayer.inventory.containerMain);
                if(Withdraw(strSteamID,double.Parse(buyPrice)))
                {
                    strMsg = string.Format(CmdBuyItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,buyPrice,"success");
                }
                else
                {
                    strMsg = string.Format(CmdBuyItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,buyPrice,"failed");
                }
                Puts(strMsg);
            }
        }
        [ConsoleCommand(CmdSellItems)]
        private void SellItems(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //shopsell 76560000000000000 itemCode num price
            //---- Command Format ----//
            var strMsg = string.Empty;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            string[] args = arg.Args;
            if(args.Length != 4)
            {
                //Invalid Parameter Numbers
                strMsg = CmdSellItems+",-,-,-,-,-";
                Puts(strMsg);
                return;
            }
            var strSteamID = args[0];
            var strItemCode = args[1];
            var amount = int.Parse(args[2]);
            var sellPrice = args[3];
            IPlayer target = _instance.covalence.Players.FindPlayerById(strSteamID);
            if(target == null)
            {
                strMsg = string.Format(CmdSellItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,sellPrice,"offline");
                Puts(strMsg);
                return;
            }
            if(!target.IsConnected)
            {
                strMsg = string.Format(CmdSellItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,sellPrice,"offline");
                Puts(strMsg);
                return;
            }
            var bplayer = target.Object as BasePlayer;

            var playerHasAmount = GetAmount(target, strItemCode);
            if(playerHasAmount < amount)
            {
                strMsg = string.Format(CmdSellItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,sellPrice,"invalidamount");
                Puts(strMsg);
            }
            else
            {
                var item = bplayer.inventory.containerMain.FindItemsByItemName(strItemCode);
                TakeItem(bplayer.inventory, bplayer.inventory.containerMain, item, amount);
                bplayer.inventory.SendUpdatedInventory(PlayerInventory.Type.Main, bplayer.inventory.containerMain);
                if(Deposit(strSteamID,double.Parse(sellPrice)))
                {
                    strMsg = string.Format(CmdSellItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,sellPrice,"success");
                }
                else
                {
                    strMsg = string.Format(CmdSellItems+",{0},{1},{2},{3},{4}",strSteamID,strItemCode,amount,sellPrice,"failed");
                }
                Puts(strMsg);
            }
        }
        [ConsoleCommand(CmdGetServerUserList)]
        private void GetServerUserList(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //gsul
            //---- Command Format ----//
            var strMsg = CmdGetServerUserList;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            IEnumerable<BasePlayer> userOnline = GetOnlineUserList();
            foreach(BasePlayer user in userOnline)
            {
                if(user.UserIDString.Length == 17)
                {
                    strMsg += string.Format(",{0}|{1}",user.UserIDString,Balance(user.UserIDString).ToString());
                }
            }
            IEnumerable<BasePlayer> userOffline = GetOfflineUserList();
            foreach(BasePlayer user in userOffline)
            {
                if(user.UserIDString.Length == 17)
                {
                    strMsg += string.Format(",{0}|{1}",user.UserIDString,Balance(user.UserIDString).ToString());
                }
            }
            Puts(strMsg);
        }
        [ConsoleCommand(CmdGetServerUserListOnline)]
        private void GetServerUserListOnline(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //gsulonline
            //---- Command Format ----//
            var strMsg = CmdGetServerUserListOnline;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            IEnumerable<BasePlayer> userOnline = GetOnlineUserList();
            foreach(BasePlayer user in userOnline)
            {
                if(user.UserIDString.Length == 17)
                {
                    strMsg += string.Format(",{0}|{1}",user.UserIDString,Balance(user.UserIDString).ToString());
                }
            }
            Puts(strMsg);
        }
        [ConsoleCommand(CmdGetServerUserListOffline)]
        private void GetServerUserListOffline(ConsoleSystem.Arg arg)
        {
            //---- Command Format ----//
            //gsuloffline
            //---- Command Format ----//
            var strMsg = CmdGetServerUserListOffline;
            BasePlayer player = arg.Player();
            if(player!=null)
            {
                //Not Server Side Action
                return;
            }
            //Is Server Side Action
            IEnumerable<BasePlayer> userOffline = GetOfflineUserList();
            foreach(BasePlayer user in userOffline)
            {
                if(user.UserIDString.Length == 17)
                {
                    strMsg += string.Format(",{0}|{1}",user.UserIDString,Balance(user.UserIDString).ToString());
                }
            }
            Puts(strMsg);
        }
        #endregion
        #region Customed Functions
        private IEnumerable<BasePlayer> GetOfflineUserList()
        {
            IEnumerable<BasePlayer> userOffline = Player.Sleepers.Where(X => !X.IsNpc && !Player.IsBanned(X.userID));
            return userOffline.OrderBy(x => x.displayName).ThenBy(x => x.userID);
        }
        private IEnumerable<BasePlayer> GetOnlineUserList()
        {
            IEnumerable<BasePlayer> userOnline = Player.Players.Where(X => !X.IsNpc && !Player.IsBanned(X.userID));
            return userOnline.OrderBy(x => x.displayName).ThenBy(x => x.userID);
        }
        private double Balance(string strSteamID)
        {
            return Economics.Call<double>("Balance", strSteamID);
        }
        private bool Deposit(string strSteamID,double amount)
        {
            return Economics.Call<bool>("Deposit", strSteamID, amount);
        }
        private bool Withdraw(string strSteamID,double amount)
        {
            return Economics.Call<bool>("Withdraw", strSteamID, amount);
        }
        private bool SetBalance(string strSteamID,double amount)
        {
            return Economics.Call<bool>("SetBalance", strSteamID, amount);
        }
        private void AddItem(ItemContainer container,string strItemCode,int amount)
        {
            if (amount == 0)
                return;
            var itemToCreate = new ItemDefinition();
            foreach (ItemDefinition item in ItemManager.itemList)
            {
                if(item.shortname == strItemCode)
                {
                    itemToCreate = item;
                }
            }
            container.AddItem(itemToCreate, amount);
        }
        private void TakeItem(PlayerInventory inventory, ItemContainer container, Item item, int amount)
        {
            var tradeAmount = amount;
            var removeList = new List<uint>();
            foreach (Item obj in container.itemList)
            {
                if(obj.info.itemid == item.info.itemid)
                {
                    var firstAmount = obj.amount;
                    if(obj.amount > tradeAmount)
                    {
                        //Amount Enough for First Stack Items
                        obj.amount -= tradeAmount;
                    }
                    else
                    {
                        //Amount Not Enough for First Stack Items
                        removeList.Add(obj.uid);
                        tradeAmount -= firstAmount;
                    }
                }
            }
            foreach (uint uid in removeList)
            {
                var removeItem = container.FindItemByUID(uid);
                container.Remove(removeItem);
            }
        }
        private int GetAmount(IPlayer player, string strItemName)
        {
            var amount = 0;
            var bplayer = player.Object as BasePlayer;
            var item = bplayer.inventory.containerMain.FindItemsByItemName(strItemName);
            if(item == null)
            {
                amount = 0;
            }
            else
            {
                amount = bplayer.inventory.containerMain.GetAmount(item.info.itemid, true);
            }
            return amount;
        }
        private string EncodeLoginNum(string strSteamID)
        {
            var strDtHour = DateTime.Now.ToString("yyyyMMddHH");
            var strData = strDtHour + strSteamID;
            var value = EncodeMD5(strData);
            for(int i=0;i<20;i++)
            {
                value = EncodeBase64(value);
            }
            var dig0 = NumOfChar(value,"V");
            var dig1 = NumOfChar(value,"B");
            var dig2 = NumOfChar(value,"W");
            var dig3 = NumOfChar(value,"D");
            var dig4 = NumOfChar(value,"X");
            var dig5 = NumOfChar(value,"F");
            var result = string.Format("{0}{1}{2}{3}{4}{5}",dig0,dig1,dig2,dig3,dig4,dig5);
            return result;
        }
        private string EncodeMD5(string strData)
        {
            var md5 = new MD5CryptoServiceProvider();
            var bytValue = Encoding.UTF8.GetBytes(strData);
            var bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            var sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToLower();
        }
        private string EncodeBase64(string strData)
        {
            var arrData = Encoding.UTF8.GetBytes(strData);
            var strBase64 = Convert.ToBase64String(arrData);
            return strBase64;
        }
        private string DecodeBase64(string strData)
        {
            string strRaw = string.Empty;
            try
            {
                byte[] bytes = Convert.FromBase64String(strData);
                strRaw = Encoding.UTF8.GetString(bytes);
            }
            catch { }
            return strRaw;
        }
        private string EncodeShopQrcodeUrl(string strShopUniqID)
        {
            var strShopQrcodeUrl = "https://rust.phellytech.com/manage/qrcode.php?data=" + EncodeBase64("https://rust.phellytech.com/onlineShopLogin.php?ShopUniqID=" + strShopUniqID);
            return strShopQrcodeUrl;
        }
        private string NumOfChar(string strSrc,string strChar)
        {
            var count = 0;
            for (int i = 0; i < strSrc.Length - strChar.Length; i++)
            {
                if (strSrc.Substring(i, strChar.Length) == strChar)
                {
                    count++;
                }
            }
            var str = count.ToString();
            return str.Substring(str.Length-1,1);
        }
        #endregion
    }
}