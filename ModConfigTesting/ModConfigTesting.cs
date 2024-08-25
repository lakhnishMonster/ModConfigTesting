using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WildfrostHopeMod;
using WildfrostHopeMod.Configs;

namespace ModConfigTesting
{
    public class ModConfigTesting : WildfrostMod
    {
        public ModConfigTesting(string modDirectory) : base(modDirectory)
        {
        }

        [ConfigItem(Variant.OptionA, "", "MyConfigs")]
        [ConfigManagerTitle("Test Config")]
        [ConfigManagerDesc("Requires game restart and new run.")]
        [ConfigOptions(new string[] { "Config Option A:\n 10,10,10 Stats", 
                                      "Config Option B:\n 5,5,5 Stats" }, 
        new object[] 
        { 
            new Variant[]
            {
                Variant.OptionA,
                Variant.OptionB
            } 
        }
        )]
        public Variant ConfiguredVariant1; //THIS MUST BE PLACED AFTER EACH CONFIGITEM OR ELSE YOU'll GET AN ERROR
        /*
        //For example, this won't work.
        //[ConfigItem(Variant.OptionA, "", "MyConfigs")]
        //[ConfigItem(Variant.OptionC, "", "MyConfigs")]
        */

        [ConfigItem(Variant.OptionC, "", "MyConfigs2")]
        [ConfigManagerTitle("Test Config")]
        [ConfigManagerDesc("Requires game restart and new run.")]
        [ConfigOptions(new string[] { "Config Option C:\n 15,15,15 Stats",
                                      "Config Option D:\n 20,20,20 Stats" },
        new object[]
        {
            new Variant[]
            {
                Variant.OptionC,
                Variant.OptionD
            }
        }
        )]
        public Variant ConfiguredVariant2;
        

        //MOD INFO
        public override string GUID => "lakhnish_monster.wildfrost.configtesting";
        public override string[] Depends => new string[1] { "hope.wildfrost.configs" };
        public override string Title => "Testing Config Settings";
        public override string Description => "This mod tests config settings";

        //CARDS SETUP
        private List<CardDataBuilder> cards;
        private List<StatusEffectDataBuilder> statusEffects;
        private bool preLoaded = false;

        //HELPER METHODS
        private T TryGet<T>(string name) where T : DataFile
        {
            T data;
            if (typeof(StatusEffectData).IsAssignableFrom(typeof(T)))
                data = base.Get<StatusEffectData>(name) as T;
            else
                data = base.Get<T>(name);

            if (data == null)
                throw new Exception($"TryGet Error: Could not find a [{typeof(T).Name}] with the name [{name}] or [{Extensions.PrefixGUID(name, this)}]");

            return data;
        }

        private CardData.StatusEffectStacks SStack(string name, int amount) => new CardData.StatusEffectStacks(Get<StatusEffectData>(name), amount);

        private StatusEffectDataBuilder StatusCopy(string oldName, string newName)
        {
            StatusEffectData data = TryGet<StatusEffectData>(oldName).InstantiateKeepName();
            data.name = GUID + "." + newName;
            StatusEffectDataBuilder builder = data.Edit<StatusEffectData, StatusEffectDataBuilder>();
            builder.Mod = this;
            return builder;
        }

        //CARD & STATUS CREATION
        private void CreateModAssets()
        {
            statusEffects = new List<StatusEffectDataBuilder>();
            //STATUS EFFECTS HERE

            cards = new List<CardDataBuilder>();
            //CARDS HERE

            //CREATING OUR FIRST BASE CARD
            CardDataBuilder myFirstCardBuilder = new CardDataBuilder(this)
                .CreateUnit("myFirstCard", "myFirstCard")
                .SetSprites("", "")
                .IsPet((ChallengeData)null, value: true);
            
            //CREATING THE DIFFERENT VARIANTS FOR WHAT OUR CARD COULD BE DEPENDING ON WHICH MOD CONFIG THE USER CHOSES
            switch(ConfiguredVariant1)
            {
                case Variant.OptionA:
                    myFirstCardBuilder.SetStats(10,10,10);
                    break;

                case Variant.OptionB:
                    myFirstCardBuilder.SetStats(5,5,5);
                    break;

                default:

                    break;
            }
            cards.Add(myFirstCardBuilder);

            //CREATING OUR SECOND BASE CARD
            CardDataBuilder mySecondCardBuilder = new CardDataBuilder(this)
            .CreateUnit("mySecondCard", "mySecondCard")
            .SetSprites("", "")
            .IsPet((ChallengeData)null, value: true);

            //CREATING THE DIFFERENT VARIANTS FOR WHAT OUR CARD COULD BE DEPENDING ON WHICH MOD CONFIG THE USER CHOSES
            switch (ConfiguredVariant2)
            {
                case Variant.OptionA:
                    mySecondCardBuilder.SetStats(15, 15, 15);
                    break;

                case Variant.OptionB:
                    mySecondCardBuilder.SetStats(20, 20, 20);
                    break;

                default:

                    break;
            }
            cards.Add(mySecondCardBuilder);


            preLoaded = true;
        }


        protected override void Load()
        {
            if (!preLoaded) { CreateModAssets(); }

            ConfigManager.OnModLoaded += HandleModLoaded;
            base.Load();
        }

        protected override void Unload()
        {
            base.Unload();
        }

        private void HandleModLoaded(WildfrostMod mod)
        {
            if (!(mod.GUID != "hope.wildfrost.configs"))
            {
                ConfigSection configSection = ConfigManager.GetConfigSection(this);
                if (configSection != null)
                {
                    configSection.OnConfigChanged += HandleConfigChange;
                }
            }
        }

        private void HandleConfigChange(ConfigItem item, object value)
        {
            Debug.Log("[ModConfigTesting] config changed!!");
        }


        public override List<T> AddAssets<T, Y>()
        {
            var typeName = typeof(T).Name;
            switch (typeName)
            {
                case nameof(CardDataBuilder):
                    return cards.Cast<T>().ToList();
                case nameof(StatusEffectDataBuilder):
                    return statusEffects.Cast<T>().ToList();
                default:
                    return null;
            }
        }


    }
}