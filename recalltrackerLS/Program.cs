using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace recalltrackerLS {
    class Program {
        static void Main(string[] args) {
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Drawing.OnDraw += Drawing_OnDraw;
            RecallList = new List<Recall>();
        }
        
        static void Drawing_OnDraw(EventArgs args) {
            float x = Drawing.Width - 605;
            float y = Drawing.Height - 150;
            
            foreach (Recall r in RecallList) {
                if (r.update()) {

                } else {
                    if (r.LastAction == Recall.RecallState.Recalling) {
                        Drawing.DrawText(x, y, System.Drawing.Color.Beige, r.ToString());
                    } else if (r.LastAction == Recall.RecallState.Ported) {
                        Drawing.DrawText(x, y, System.Drawing.Color.GreenYellow, r.ToString());
                    } else if (r.LastAction == Recall.RecallState.Cancelled) {
                        Drawing.DrawText(x, y, System.Drawing.Color.Red, r.ToString());
                    }
                    y += 15;
                }
            }
        }
        static void Recall_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            
        }

        static List<Recall> RecallList;

        static void Game_OnGameProcessPacket(GamePacketEventArgs args) {
            if (args.PacketData[0] == 0xD8 || args.PacketData[0] == 0xD7) {
                var stream = new System.IO.MemoryStream(args.PacketData);
                var byteRead = new System.IO.BinaryReader(stream);

                byteRead.ReadByte();
                byteRead.ReadBytes(4);
                var netidbytes = byteRead.ReadBytes(4);
                var networkId = System.BitConverter.ToInt32(netidbytes, 0);

                byteRead.ReadBytes(0x42);
                string s = System.BitConverter.ToString(byteRead.ReadBytes(6));
                var state = Recall.RecallState.Recalling;
                if (string.Equals("00-00-00-00-00-00", s)) {
                    state = Recall.RecallState.Unknown;
                }
                byteRead.Close();

                var unit = ObjectManager.GetUnitByNetworkId<GameObject>(networkId);
                if (unit == null || !unit.IsValid) {
                    return;
                }
                if (unit.Team == ObjectManager.Player.Team) {
                    return;
                }
                
                HandleRecall((Obj_AI_Hero)unit, state);
            }
        }
        public static void HandleRecall(Obj_AI_Hero hero, Recall.RecallState recallType) {
            Recall recall = null;
            recall = RecallList.Where(x => x.Player.NetworkId == hero.NetworkId).FirstOrDefault();
            if (recall == null) {
                recall = new Recall(hero);
                recall.LastAction = recallType;
                RecallList.Add(recall);
            } else {
                recall.LastAction = recallType;
            }
        }
    }
}
