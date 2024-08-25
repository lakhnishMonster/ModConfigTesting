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
        
        public Variant ConfiguredVariant;

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

            //CREATING OUR BASE CARD
            CardDataBuilder myCardBuilder = new CardDataBuilder(this)
                .CreateUnit("myVariantCard", "myVariantCard")
                .SetSprites("", "")
                .IsPet((ChallengeData)null, value: true);
            
            //CREATING THE DIFFERENT VARIANTS FOR WHAT OUR CARD COULD BE DEPENDING ON WHICH MOD CONFIG THE USER CHOSES
            switch(ConfiguredVariant)
            {
                case Variant.OptionA:
                    myCardBuilder.SetStats(10,10,10);
                    break;

                case Variant.OptionB:
                    myCardBuilder.SetStats(5,5,5);
                    break;

                default:

                    break;
            }

            cards.Add(myCardBuilder);
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