using System;
using System.Linq;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using HistoryKit;
using XRL.Language;
using SimpleJSON;

namespace XRL.World.Parts
{
    public class acegiak_RomancePreferenceResult{
        public string explanation;
        public float amount;
        public acegiak_RomancePreferenceResult(float amount,string explanation){
            this.amount = amount;
            this.explanation = explanation;
        }
    }

	[Serializable]
	public abstract class acegiak_RomancePreference
	{
        [NonSerialized]
        public acegiak_Romancable Romancable = null;
        public abstract acegiak_RomancePreferenceResult GiftRecieve(GameObject from, GameObject gift);
        public abstract acegiak_RomanceChatNode BuildNode(acegiak_RomanceChatNode node);

        public abstract string GetStory(acegiak_RomanceChatNode node, HistoricEntitySnapshot entity);
        public abstract acegiak_RomancePreferenceResult DateAssess(GameObject Date, GameObject DateObject);

        public abstract string getstoryoption(string key);
        public void setRomancable(acegiak_Romancable romancable){
            this.Romancable = romancable;
        }
        public virtual void Save(SerializationWriter Writer){
            Writer.Write(this.GetType().FullName);
        }
        public virtual void Load(SerializationReader Reader){
        }
        
        public static void Read(SerializationReader Reader, acegiak_Romancable romancable){
            string classname = Reader.ReadString();
            Type type = Type.GetType(classname);
            acegiak_RomancePreference preference = (acegiak_RomancePreference)Activator.CreateInstance(type,romancable);
            preference.setRomancable(romancable);
            preference.Load(Reader);
            romancable.preferences.Add(preference);             
        }

        // Utility
        public static void SetSampleObject(
            Dictionary<string, string> vars,
            GameObject                 obj,
            string                     defaultName = "thing",
            bool                       withColor = false,
            char                       postColor = '\0',
            char                       preColor = '\0')
        {
            if (obj != null)
            {
                string sample = (withColor ? obj.DisplayNameOnlyDirect : obj.DisplayNameOnlyDirectAndStripped);
                string samples = Grammar.Pluralize(sample);
                if (postColor != '\0')
                {
                    sample += "&"+postColor;
                    samples += "&"+postColor;
                }
                string pre = "";
                if (preColor != '\0') pre = "&"+preColor;
                vars["*sample*"]   = pre + sample;
                vars["*samples*"]  = pre + samples;
                vars["*a_sample*"] = pre + obj.a + sample;
                vars["*A_sample*"] = pre + obj.A + sample;
                vars["*the_sample*"] = pre + obj.the + sample;
                vars["*The_sample*"] = pre + obj.The + sample;
                vars["*the_samples*"] = pre + obj.the + samples;
                vars["*The_samples*"] = pre + obj.The + samples;
            }
            else
            {
                string sample = defaultName;
                string samples = Grammar.Pluralize(sample);
                vars["*sample*"]   = sample;
                vars["*samples*"]  = samples;
                vars["*a_sample*"] = "a " + sample;
                vars["*A_sample*"] = "A " + sample;
                vars["*the_sample*"] = "the " + sample;
                vars["*The_sample*"] = "The " + sample;
                vars["*the_samples*"] = "the " + samples;
                vars["*The_samples*"] = "The " + samples;
            }
        }
        
        public acegiak_RomanceChatNode Build_QA_Node(
            acegiak_RomanceChatNode node,
            string qa_path,
            string gen_type,
            Dictionary<string, string> vars,
            HistoricEntitySnapshot     entity = null)
        {
            var qa_text = new QA_Node_Helper(qa_path, entity, vars);

            string code = qa_text.Get(gen_type);

            acegiak_RomanceText.Log("Q&A Script: " + code);

            string[] parts = code.Split('|');
            if (parts.Count() < 1) acegiak_RomanceText.Log("ERROR: Q&A blank: " + code);

            string bodytext;

            if (parts[0] == "Q")
            {
                if (parts.Count() < 3)
                    acegiak_RomanceText.Log("ERROR: Q&A too few params in quiz: " + code);
                
                // Generated question.
                bodytext = qa_text.Get(parts[1]);

                for (int i = 2; i < parts.Count(); ++i)
                {
                    qa_text.MakeChoiceFromCode(node, parts[i]);
                }
            }
            else if (parts[0] == "S")
            {
                if (parts.Count() != 5)
                    acegiak_RomanceText.Log("ERROR: Q&A need 5 params in show-and-tell: " + code);

                // 1: Generated question.
                bodytext = qa_text.Get(parts[1]);

                // 2: Item types to include and exclude
                var item_filter = new acegiak_ItemFilter(parts[2]);

                // 3: Paths for show prompt and reactions
                string   choice_show;
                string[] react_text  = new string[3];
                int[]    react_value = new int   [3];
                {
                    string[] subparts = parts[3].Split(',');

                    if (subparts.Count() != 7)
                        acegiak_RomanceText.Log("ERROR: Q&A need 7 params in show section: " + parts[3]);

                    choice_show = subparts[0];

                    for (int i = 0; i < 3; ++i)
                    {
                        react_text [i] = qa_text.Get(choice_show + "." + subparts[1 + 2*i]);
                        react_value[i] = 0;
                        if (!int.TryParse(subparts[2 + 2*i], out react_value[i]))
                            acegiak_RomanceText.Log("ERROR: Q&A non-integer choice value: " + code);
                    }
                }

                // 3b: Generate list of showable items.
                List<GameObject> showables = XRLCore.Core.Game.Player.Body.GetPart<Inventory>().GetObjects();
                foreach (BodyPart item in XRLCore.Core.Game.Player.Body.GetPart<Body>().GetEquippedParts())
                {
                    if(item.Equipped != null){
                        showables.Add(item.Equipped);
                    }
                }

                // Filter and sort the items into buckets assessed quality.
                var itemsGood = new List<GameObject>();
                var itemsNeut = new List<GameObject>();
                var itemsBad  = new List<GameObject>();
                foreach(GameObject item in showables)
                {
                    if (!item_filter.Passes(item)) continue;

                    var amount = Romancable.assessGift(item,XRLCore.Core.Game.Player.Body).amount;
                    int kind = 1;
                    if      (amount > 0f) itemsGood.Add(item);
                    else if (amount < 0f) itemsBad .Add(item);
                    else                  itemsNeut.Add(item);
                }

                // Make a set of up to 5 random choices (with value noted)
                //   Include at least one good and one bad option if possible.
                var options = new Dictionary<GameObject, int>();
                if (itemsGood.Count() != 0)
                {
                    int index = (Stat.Rnd2.Next() % itemsGood.Count());
                    options.Add(itemsGood.RemoveRandomElement(Stat.Rnd2), 0);
                    itemsGood.RemoveAt(index);
                }
                if (itemsBad .Count() != 0)
                {
                    int index = (Stat.Rnd2.Next() % itemsBad.Count());
                    options.Add(itemsBad .RemoveRandomElement(Stat.Rnd2), 2);
                    itemsBad .RemoveAt(index);
                }
                while (options.Count() < 5)
                {
                    int
                        good   = itemsGood.Count(),
                        notbad = itemsNeut.Count() + good,
                        total  = itemsBad .Count() + notbad;
                    if (total == 0) break;

                    int index = (Stat.Rnd2.Next() % total);
                    if      (index < good)
                        {options.Add(itemsGood[index], 0);        itemsGood.RemoveAt(index);}
                    else if (index < notbad)
                        {options.Add(itemsNeut[index-good], 1);   itemsNeut.RemoveAt(index-good);}
                    else
                        {options.Add(itemsBad [index-notbad], 2); itemsBad .RemoveAt(index-notbad);}
                }

                // Now place choices in random order.
                int optionNumber = 1;
                while (options.Count() > 0)
                {
                    int index = (Stat.Rnd2.Next() % options.Count());
                    var option = options.ElementAt(index);
                    options.Remove(option.Key);
                    
                    SetSampleObject(vars, option.Key, "item", true, 'g', 'y');

                    node.AddChoice(choice_show + optionNumber.ToString(),
                        qa_text.Get(choice_show + ".a"),
                        react_text [option.Value],
                        react_value[option.Value]);
                }

                // 4: Generate neutral response
                qa_text.MakeChoiceFromCode(node, parts[4]);
            }
            else
            {
                bodytext = "&RI have failed to think of an interesting question!&y";
                node.AddChoice("ok", "How unfortunate.", "A terrible shame indeed!", 0);
            }

            // Insert story
            if(Romancable != null){
                node.Text = node.Text+"\n\n"+Romancable.GetStory(node);
            }
            node.Text = node.Text+"\n\n"+bodytext;

            return node;
        }

        class acegiak_ItemFilter
        {
            Dictionary<string, bool> part_types;
            Dictionary<string, bool> blueprints;

            public acegiak_ItemFilter(string filterCode)
            {
                part_types = new Dictionary<string, bool>();
                blueprints = new Dictionary<string, bool>();
                foreach(var str in filterCode.Split(','))
                {
                    switch (str.First())
                    {
                    default:  part_types.Add(str,              true);  break;
                    case '+': part_types.Add(str.Substring(1), true);  break;
                    case '-': part_types.Add(str.Substring(1), false); break;
                    case '^': blueprints.Add(str.Substring(1), true);  break;
                    case '~': blueprints.Add(str.Substring(1), false); break;
                    }
                }
                if (part_types.Count() == 0) part_types = null;
                if (blueprints.Count() == 0) blueprints = null;
            }

            public bool Passes(GameObject item)
            {
                acegiak_RomanceText.Log("Testing item: " + item.DisplayNameOnlyDirectAndStripped);
                // Part-type test
                if (part_types != null)
                {
                    bool pass = false;
                    foreach(var part in item.PartsList) // Faster to loop through parts
                    {
                        if (part_types.ContainsKey(part.Name))
                        {
                            acegiak_RomanceText.Log("  Part " + part.Name);
                            if (part_types[part.Name]) pass = true;
                            else                       return false;
                        }
                    }
                    if (!pass) {acegiak_RomanceText.Log("  Fails part types."); return false;}
                }
                // Blueprint inheritance test
                if (blueprints != null)
                {
                    bool pass = false;
                    for (string blueprint = item.Blueprint;
                        !string.IsNullOrEmpty(blueprint);
                        blueprint = GameObjectFactory.Factory.Blueprints[blueprint].Inherits)
                    {
                        acegiak_RomanceText.Log("  ...is a " + blueprint);
                        if (blueprints.ContainsKey(blueprint))
                        {
                            if (blueprints[blueprint]) pass = true;
                            else                       return false;
                        }
                    }
                    if (!pass) {acegiak_RomanceText.Log("  Fails blueprints."); return false;}
                }
                acegiak_RomanceText.Log("  Passes.");
                return true;
            }

            public List<GameObject> Filter(List<GameObject> list)
            {
                var results = new List<GameObject>();
                foreach(GameObject item in list)
                    if (Passes(item)) results.Add(item);
                return results;
            }
        }

        private class QA_Node_Helper
        {
            string                     qa_base;
            HistoricEntitySnapshot     entity;
            Dictionary<string, string> vars;

            public QA_Node_Helper(string qa_path, HistoricEntitySnapshot entity, Dictionary<string, string> vars)
            {
                qa_base = "<spice.eros.opinion." + qa_path + ".";
                this.entity = entity;
                this.vars = vars;
            }

            public string Get(string sub_path)
            {
                string tag = qa_base + sub_path + ".!random>";
                string result = acegiak_RomanceText.ExpandString(tag, entity, vars);
                if (result.Count() > 0) return result;
                
                acegiak_RomanceText.Log("ERROR: Q&A expansion failed for " + tag);
                return "&R" + tag.Substring(1,tag.Count()-1) + "&y";
            }

            public void MakeChoice(acegiak_RomanceChatNode node,
                string choice_path, string result_type, int result_value)
            {
                node.AddChoice(choice_path,
                    Get(choice_path + ".a"),
                    Get(choice_path + "." + result_type), result_value);
            }

            public void MakeChoiceFromCode(acegiak_RomanceChatNode node, string code)
            {
                string[] subparts = code.Split(',');
                if (subparts.Count() != 3)
                {
                    acegiak_RomanceText.Log("ERROR: Q&A choice needs 3 param: `" + code);
                    return;
                }

                int result_value = 0;
                if (!int.TryParse(subparts[2], out result_value))
                    acegiak_RomanceText.Log("ERROR: Q&A param 3 is not integer: `" + code);

                MakeChoice(node, subparts[0], subparts[1], result_value);
            }
        }
    }
}