using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using SEHI;


/*   
 * To execute this script, run the following command from the chat window:
        //call SerialCommsOut_SerialCommsOut SerialCommsOut.Script ShowSerialVars
 */


//Project Goal: Try to do all math here before sending data to Hardware to mitigate slower external HW clocks.

namespace SerialCommsOut 
{
    
 

    //We need to create a class object that contains all of the output properties we want to send to the Serial Port 
    //We are creating these as strings so that the Hardware script can grab the data from the Serial buffer without 
    //having to know the type ahead of time. This allows the Hardware to continue to recieve data while it is being read.
    //Getting Typed data from the serial buffer silently blocks new data until the read is complete, which could lead to lost data.
    public class SerialVars
    {
        public int ItemID { get; set; }
        public string DataType { get; set; } // Type of data. Float, Int, string, etc. Different Hardware components may need to math some stuff to display data properly. 
        public string ItemName { get; set; } //friendly name i.e. "health"
        public int CurrentValue { get; set; } //health percentage etc.
        public string MaxValue { get; set; } // Maximum range for determining what to set the Analog pin value to in Hardware  
        public string lastState { get; set; }
   
    }
    public class MonsterObject
    {
        private Dictionary<string, int?> _lastStates = new Dictionary<string, int?>();

        int? value = null;

        public bool GetCurrentLasState(SerialVars serialVars)
        {
            if (!_lastStates.TryGetValue(serialVars.lastState, out value))
            {
                _lastStates.Add(serialVars.lastState, serialVars.CurrentValue); // Add to dictionary  - send data 
                return true;
            }
            else
            {
                if (serialVars.CurrentValue != value)
                {
                    _lastStates[serialVars.lastState] = serialVars.CurrentValue; // Updating dictionary - send data 
                    return true;
                }
            }

            // not a new value and not an updated value - don't send data
            return false;
        }
  
        
    }
        
        

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

     class Script : MySessionComponentBase
    {
        public static int CurrentPlayerState{get; set;}
        public static IMyEntity EntityStash { get; set; }
        public static MonsterObject LastStates = new MonsterObject();

        #region Get Player Data
        //Add switch case for CurrentPlayerState for each method below in this region only. 
        //If CurrentPlayerState !=0 pass the stashed Player Entity - Needs to be written.

        static public string GetPlayerHealth(MyObjectBuilder_Character CharacterInfo)
        {
            float? percentFloat = null;
            if(CurrentPlayerState == 0)
            {
                if (!CharacterInfo.Health.HasValue)
                {
                    percentFloat = 100.00f;
                }
                else
                {
                    percentFloat = CharacterInfo.Health.Value;
                }
            }
            else
            {
                percentFloat = 666666;
            }

           return percentFloat.ToString();   
        } 
        static public string GetPlayerEnergy(MyObjectBuilder_Character CharacterInfo)
        {
            double percentDouble = 0;
            if (CurrentPlayerState == 0)
            {
                percentDouble = CharacterInfo.Battery.CurrentCapacity * 10000000f;
            }
            return percentDouble.ToString();
         }
        static public string GetPlayerOxygen(MyObjectBuilder_Character CharacterInfo)
        {
            double percentDouble = 0;
            if (CurrentPlayerState == 0)
            {
                 percentDouble = CharacterInfo.OxygenLevel * 10000f / 100;
            }
            return percentDouble.ToString();            
        }
        //static public string GetPlayerHydrogen(MyObjectBuilder_Character CharacterInfo)
        //{
        //    double percentDouble = 0;
        //    string currentamount = string.Empty;
        //    string id = string.Empty;
        //    if (CurrentPlayerState == 0)
        //    {
        //        //percentDouble = CharacterInfo.StoredGases..Capacity (* 10000f / 100);
        //        List<MyObjectBuilder_Character.StoredGas> StoredGases = CharacterInfo.StoredGases;
        //        foreach (MyObjectBuilder_Character.StoredGas gas in StoredGases)
        //        {
        //             id = gas.Id.ToString();
        //             if (id == "MyObjectBuilder_GasProperties/Hydrogen")
        //             { percentDouble = gas.FillLevel *100; }
        //             else { percentDouble = 0; }

        //        }
        //    }
        //    return percentDouble.ToString();
        //}
        static public string GetPlayerDamperStatus(MyObjectBuilder_Character CharacterInfo)
        {
            bool BitVal = false;
            if (CurrentPlayerState == 0)
            {
                BitVal = CharacterInfo.DampenersEnabled;
            }
            return BitVal.ToString();
        }  
        static public string GetPlayerJetPackStatus(MyObjectBuilder_Character CharacterInfo)
        {
            bool BitVal = false;
            if (CurrentPlayerState == 0)
            {
                BitVal = CharacterInfo.JetpackEnabled;
            }
            return BitVal.ToString();
        }
        static public string GetPlayerSuitLightStatus(MyObjectBuilder_Character CharacterInfo)
        {
            bool BitVal = false;
            if (CurrentPlayerState == 0)
            {
                BitVal = CharacterInfo.LightEnabled;
            }
            return BitVal.ToString();
        }
        static public string GetPlayerSpeed(MyObjectBuilder_Character CharacterInfo)
        {
            string StringVal = string.Empty;
            if (CurrentPlayerState == 0)
            { 
            float Xvalue = CharacterInfo.LinearVelocity.X;
            float Yvalue = CharacterInfo.LinearVelocity.Y;
            float Zvalue = CharacterInfo.LinearVelocity.Z;
            Vector3 testVector = new Vector3(Xvalue, Yvalue, Zvalue);
            StringVal = testVector.Length().ToString() + " m/s";
            }
            return StringVal;
        }
        static public string GetPlayerMaxInventory()
        {
            var characterEntity = (MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity);
            //var invOwner = (characterEntity as VRage.Game.ModAPI.Ingame.IMyInventoryOwner);
            float InvVol = 0;
            if (characterEntity != null && characterEntity.InventoryCount > 0)
            {
                var inv = characterEntity.GetInventory(0);

                if (CurrentPlayerState == 0)
                {
                    InvVol = (float)inv.MaxVolume;
                    //values[Icons.INVENTORY] = ((float)inv.CurrentVolume / (float)inv.MaxVolume) * 100;
                }
                else
                {
                    InvVol = 4000F;
                }
                    
            }                               
            return InvVol.ToString();
        }
        static public string GetPlayerCurrentInventory()
        {
            var characterEntity = (MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity);
            //var invOwner = (characterEntity as VRage.Game.ModAPI.Ingame.IMyInventoryOwner);
            float InvVol = 0;
            if (characterEntity != null && characterEntity.InventoryCount > 0)
            {
                var inv = characterEntity.GetInventory(0);

                if (CurrentPlayerState == 0)
                {
                    InvVol = (float)inv.CurrentVolume;
                    //values[Icons.INVENTORY] = ((float)inv.CurrentVolume / (float)inv.MaxVolume) * 100;
                }
                else
                {
                    InvVol = 4000F;
                }

            }
            return InvVol.ToString();
        }
        static public string GetPlayerAntennaStatus(MyObjectBuilder_Character CharacterInfo)
        {
            bool BitVal = false;
            if (CurrentPlayerState == 0)
            {
                BitVal = CharacterInfo.EnableBroadcasting;
            }
            return BitVal.ToString();
        }
    #endregion

        #region Get Ship Data
        static public string GetShipSomething()
        {
            string SomeShipData = string.Empty;
            if (CurrentPlayerState > 0)
            {


            }
            return SomeShipData;
        }
        #endregion


        static public int GetPlayerState()
        {
            int PlayerState = 1;
            //check to see if player is solo = 0, in a cockpit = 1, chair = 2 or Cryopod = 3
            //doing this in case we want a different output set depending on the situation (ex: we won't allow block/ship control for player in cryopod)   
            VRage.ModAPI.IMyEntity controlled = null;
            string state = string.Empty;
            try
            {
                 controlled = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity;
            }
            catch
            {
                if (EntityStash != null)
                {
                     controlled = EntityStash;
                }
            }
           // var controlled = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity;

            if (controlled is VRage.Game.ModAPI.IMyCharacter)
            {
                state = "character";
                PlayerState = 0;
            }
            else if (controlled is Sandbox.ModAPI.Ingame.IMyCockpit)
            {
                state = "cockpit";
                PlayerState = 1;
            }
            else if (!(controlled is VRage.Game.ModAPI.IMyCharacter) && (!(controlled is Sandbox.ModAPI.Ingame.IMyCockpit)))
            {
                state = controlled.ToString();
                PlayerState = 3;
            }
            return PlayerState;
        }

      // ShowHelloWorld must be public static, you can define your own methods,
      // but to be able to call them from chat they must be public static 
        public static void ShowSerialVars()
        {
            CurrentPlayerState = GetPlayerState();
            try
            {
                EntityStash = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity;
            }
            catch
            {
                EntityStash = null;
            }
            MyAPIGateway.Utilities.ShowNotification("Data Collection Started", 10000, MyFontEnum.Red);            
            if(CurrentPlayerState == 0 || EntityStash != null)
            {
                
                ShowPlayerData();
            }
            
            //MyAPIGateway.Utilities.ShowNotification(state, 10000, MyFontEnum.Green);
            //MyAPIGateway.Utilities.ShowNotification(MyAPIGateway.Session.Player.ToString(), 10000, MyFontEnum.Green);
            //MyAPIGateway.Utilities.ShowNotification(MyAPIGateway.Session.Player.Client.ToString(), 10000, MyFontEnum.Green);
            //MyAPIGateway.Utilities.ShowNotification(MyAPIGateway.Session.Player.IdentityId.ToString(), 10000, MyFontEnum.Green);
            //MyAPIGateway.Utilities.ShowNotification(MyAPIGateway.Session.Player.Controller.ToString(), 10000, MyFontEnum.Green);
            //MyAPIGateway.Utilities.ShowNotification(MyAPIGateway.Session.Player.Controller.ControlledEntity.ToString(), 10000, MyFontEnum.Green);
            //MyAPIGateway.Utilities.ShowNotification(MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.ToString(), 10000, MyFontEnum.Green);
            //MyAPIGateway.Utilities.ShowNotification("Data Collection Completed", 10000, MyFontEnum.Red);
            
        }
        //DataTypes:
        //0 = Bool
        //1 = Float
        //2 = Int
        private static void ShowPlayerData()
        {
            //Temporary Arduino Values
            int ComPortNumber = 6;
            int BaudRate = 115200;
            MyObjectBuilder_Character CharacterInfo = (MyObjectBuilder_Character)MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetObjectBuilder();
            MySerialOut MessageOut = new MySerialOut();
            bool result = false;
            Dictionary<int, SerialVars> SerialOutValues = new Dictionary<int, SerialVars>();
            
            #region object gathering

            //SerialVars PlayerHealth = new SerialVars()
            //{
            //    ItemID = 0,
            //    ItemName = "PlayerHealth",
            //    CurrentValue = (int)(float.Parse(SerialCommsOut.Script.GetPlayerHealth(CharacterInfo)) * 100.0f),
            //    MaxValue = "100",
            //    DataType = "1"
            //};
            SerialVars PlayerEnergy = new SerialVars()
            {
                ItemID = 1,
                ItemName = "PlayerEnergy",
                CurrentValue = (int)(CharacterInfo.Battery.CurrentCapacity * 100),
                MaxValue = "100",
                DataType = "1",
                lastState = "LastPlayerEnergyUpdate"
            };
            SerialVars PlayerOxygen = new SerialVars()
            {
                ItemID = 2,
                ItemName = "PlayerOxygen",
                CurrentValue = (int)(CharacterInfo.OxygenLevel * 100),
                MaxValue = "100",
                DataType = "1",
                lastState = "LastPlayerOxygenUpdate"
            };
            ////SerialVars PlayerHydrogen = new SerialVars()
            ////{
            ////    ItemID = 3,
            ////    ItemName = "PlayerHydrogen",
            ////    CurrentValue = SerialCommsOut.Script.GetPlayerHydrogen(CharacterInfo),
            ////    MaxValue = "100",
            ////    DataType = "Double"
            ////};
            //SerialVars PlayerDampers = new SerialVars()
            //{
            //    ItemID = 4,
            //    ItemName = "PlayerDampers",
            //    CurrentValue = Convert.ToInt16(SerialCommsOut.Script.GetPlayerDamperStatus(CharacterInfo)),
            //    MaxValue = "1",
            //    DataType = "0"
            //};
            //SerialVars PlayerJetPack = new SerialVars()
            //{
            //    ItemID = 5,
            //    ItemName = "PlayerJetPack",
            //    CurrentValue = Convert.ToInt16(SerialCommsOut.Script.GetPlayerJetPackStatus(CharacterInfo)),
            //    MaxValue = "1",
            //    DataType = "0"
            //};
            //SerialVars PlayerSuitLight = new SerialVars()
            //{
            //    ItemID = 6,
            //    ItemName = "PlayerSuitLight",
            //    CurrentValue = Convert.ToInt16(SerialCommsOut.Script.GetPlayerSuitLightStatus(CharacterInfo)),
            //    MaxValue = "1",
            //    DataType = "0"
            //};
            //SerialVars PlayerSpeed = new SerialVars()
            //{
            //    ItemID = 7,
            //    ItemName = "PlayerSpeed",
            //    CurrentValue = (int)(float.Parse(SerialCommsOut.Script.GetPlayerSpeed(CharacterInfo)) * 100.0f),
            //    MaxValue = "0",
            //    DataType = "1"
            //};
            //SerialVars PlayerCurrentInventory = new SerialVars()
            //{
            //    ItemID = 8,
            //    ItemName = "PlayerCurrentInventory",
            //    CurrentValue = Convert.ToInt16(SerialCommsOut.Script.GetPlayerCurrentInventory()),
            //    MaxValue = "0",
            //    DataType = "2"
            //};
            //SerialVars PlayerMaxInventory = new SerialVars()
            //{
            //    ItemID = 9,
            //    ItemName = "PlayerMaxInventory",
            //    CurrentValue = Convert.ToInt16(SerialCommsOut.Script.GetPlayerMaxInventory()),
            //    MaxValue = "0",
            //    DataType = "2"
            //};
            //SerialVars PlayerAntenna = new SerialVars()
            //{
            //    ItemID = 10,
            //    ItemName = "PlayerAntenna",
            //    CurrentValue = Convert.ToInt16(SerialCommsOut.Script.GetPlayerAntennaStatus(CharacterInfo)),
            //    MaxValue = "1",
            //    DataType = "0"
            //};
            //Should Ship data ItemID always come after player data?
            //Should we break this API script into 2 Scripts one for Player and one for Ship?
            //A ship only script would be easier to attach to a programmable block until Ondrej enables local scripts

            /*SerialVars ShipDamage = new SerialVars()
            {
                ItemID = 100,
                ItemName = "ShipDamage",
                CurrentValue = "50",
                MaxValue = "100",
                DataType = "Double"
            };*/
            #endregion

            //Add all Desired Objects here with "SerialOutValues.Add"
         
                SerialOutValues.Clear();
                //SerialOutValues.Add(PlayerHealth.ItemID, PlayerHealth);
                SerialOutValues.Add(PlayerEnergy.ItemID, PlayerEnergy);
                SerialOutValues.Add(PlayerOxygen.ItemID, PlayerOxygen);
                //SerialOutValues.Add(PlayerHydrogen.ItemID, PlayerHydrogen);
                //SerialOutValues.Add(PlayerDampers.ItemID, PlayerDampers);
                //SerialOutValues.Add(PlayerJetPack.ItemID, PlayerJetPack);
                //SerialOutValues.Add(PlayerSuitLight.ItemID, PlayerSuitLight);
                //SerialOutValues.Add(PlayerSpeed.ItemID, PlayerSpeed);
                //SerialOutValues.Add(PlayerCurrentInventory.ItemID, PlayerCurrentInventory);
                //SerialOutValues.Add(PlayerMaxInventory.ItemID, PlayerMaxInventory);
                //SerialOutValues.Add(PlayerAntenna.ItemID, PlayerAntenna);
                
                foreach (SerialVars serialVars in SerialOutValues.Values)  //.OrderBy()
                {
            //        ScreenData += (String.Format("ItemID = {0}, ItemName = {1}, CurrentValue = {2}, MaxValue = {3}, DataType = {4}", serialVars.ItemID, serialVars.ItemName, serialVars.CurrentValue, serialVars.MaxValue, serialVars.DataType)) + "/\n";
                   // MessageOut = (serialVars.DataType, serialVars.ItemName, serialVars.CurrentValue, serialVars.MaxValue );
                   IMySerialOutAccess OutData = MessageOut;
                    string tempMessage = serialVars.DataType + "," + serialVars.ItemID.ToString() + "," + serialVars.CurrentValue.ToString() + "," + serialVars.MaxValue;
                    if(LastStates.GetCurrentLasState(serialVars)) // if true send message 
                    {
                        result = OutData.SendMessageToSerial(tempMessage, ComPortNumber, BaudRate);
                        MyAPIGateway.Utilities.ShowNotification(tempMessage, 3000, MyFontEnum.Green);
                    }

                    
                }
            
            //MyAPIGateway.Utilities.ShowMissionScreen("Serial Out Data", DateTime.Now.ToString(), "Values:", ScreenData, null, "Hide Screen");        
       
            /*if (CurrentPlayerState != 0)
            {
                SerialOutValues.Add(ShipDamage.ItemID, ShipDamage);
            }*/
            //call SerialCommsOut_SerialCommsOut SerialCommsOut.Script ShowSerialVars

        }
        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
          ShowPlayerData();
        }
   }
}
