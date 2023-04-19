using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static BoxExport.Program;

namespace BoxExport
{
    internal class Program
    {
        private static JToken ReadFile(string filename)
        {
            return JToken.Parse(File.ReadAllText(filename));
        }
        public enum ePromotionLevel
        {
            Bronze = 1,
            Copper1 = 2,
            Copper2 = 3,
            Silver1 = 4,
            Silver2 = 5,
            Silver3 = 6,
            Gold1 = 7,
            Gold2 = 8,
            Gold3 = 9,
            Gold4 = 10,
            Purple1 = 11,
            Purple2 = 12,
            Purple3 = 13,
            Purple4 = 14,
            Purple5 = 15,
            Purple6 = 16,
            Purple7 = 17,
            Red1 = 18,
            Red2 = 19,
            Red3 = 20,
            Green1 = 21,
            Green2 = 22,
            Green3 = 23,
            Green4 = 24,
            INVALID_VALUE = -1
        }

        private static int ParsePromotionLevel(string s)
        {
            return (int)(ePromotionLevel)Enum.Parse(typeof(ePromotionLevel), s);
        }
        public static void Main(string[] args)
        {
            var units = ReadFile("Data/unit_data.json")["RECORDS"]
                .ToDictionary(t => t.Value<int>("unit_id"), t => t.Value<string>("unit_name"));
            var items = ReadFile("Data/item_data.json")["RECORDS"]
                .ToDictionary(t => t.Value<int>("item_id"), t => t.Value<string>("item_name"));
            var equips = ReadFile("Data/equipment_data.json")["RECORDS"]
                .ToDictionary(t => t.Value<int>("equipment_id"), t => t.Value<string>("equipment_name"));

            var props = new string[]
            {
                "Hp", "Atk", "Def", "MagicStr", "MagicDef", "PhysicalCritical", "MagicCritical", "HpRecoveryRate", "LifeSteal", "Dodge", "EnergyReduceRate", "EnergyRecoveryRate", "Accuracy"
            };
            
            using (var sw = new StreamWriter(File.Open("result.csv", FileMode.Create), Encoding.GetEncoding("GB2312")))
            {
                // rarity/promotion/equip/lv/ub/s1/s2/ex/ue/love
                sw.WriteLine(",,,,," + string.Join(",",units.Values.Select(u => $"{u},,,,,,{string.Concat(props.Select(_ => ","))}")));
                sw.WriteLine("name,viewerid,free,pay,mana," + string.Join(",", units.Values.Select(u => $"rarity,rank,equip,level,ub/s1/s2/ex,ue,love{string.Concat(props.Select(p => $",{p}"))}"))
                                              + "," + string.Join(",", items.Values.Concat(equips.Values)));
                foreach (var data in Directory.GetFiles(".").Where(f => f.EndsWith(".json")).Select(ReadFile))
                {
                    sw.WriteLine(string.Join(",",
                        ($"{data["UserInfo"].Value<string>("UserName").Replace(",", "_")},{data["UserInfo"]["ViewerId"]},{data["FreeJewel"]},{data["PaidJewel"]}," +
                         $"{data["TotalGold"]}," +
                         $"{string.Join(",", units.Select(p => { var unit = data["UnitParameterDictionary"][p.Key.ToString()]?["UniqueData"]; return unit == null ? $",,,,,,{string.Concat(props.Select(_ => ","))}" : $"{unit["UnitRarity"]},{ParsePromotionLevel(unit["PromotionLevel"].ToString())},{string.Concat(unit["EquipSlot"].Select(e => e.Value<bool>("IsSlot") ? e.Value<int>("EnhancementLevel").ToString() : "-"))},{unit["UnitLevel"]},{unit["UnionBurst"][0]["SkillLevel"]}/{(unit["MainSkill"].Any() ? unit["MainSkill"][0]["SkillLevel"] : 0)}/{(unit["MainSkill"].Skip(1).Any() ? unit["MainSkill"][1]["SkillLevel"] : 0)}/{(unit["ExSkill"].Any() ? unit["ExSkill"][0]["SkillLevel"] : 0)},{(unit["UniqueEquipSlot"].Any() ? unit["UniqueEquipSlot"][0]["EnhancementLevel"] : 0)},{data["CharaParameterDictionary"][(p.Key / 100).ToString()]["LoveLevel"]}{string.Concat(props.Select(p0 => $",{unit[$"Total{p0}"]}"))}"; })) }," +
                         $"{string.Join(",", items.Select(p => data["itemDictionary"][p.Key.ToString()]?.Value<int>("Stock") ?? 0))}," +
                         $"{string.Join(",", equips.Select(p => data["equipDictionary"][p.Key.ToString()]?.Value<int>("Stock") ?? 0))}")
                        .Split(',').Select(s => $"\t{s}")));
                }
            }
        }
    }
}
