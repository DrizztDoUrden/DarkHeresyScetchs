﻿using HeresyCore.Entities.Data.Interfaces;
using HeresyCore.Entities.Enums;
using HeresyCore.Entities.Properties.Moddifiers;
using System.Collections.Generic;

namespace HeresyCore.Entities.Data.Extensions
{
    public static class CharacterExtensions
    {
        public static Character AddStats(this Character character, IStatsContainer stats)
        {
            foreach (var stat in stats.Stats)
            {
                var charStat = character.Stats[stat.Key];
                var statDice = stat.Value;

                charStat.Moddifiers.Add(stats.GroupTypeName, stat.Value.Constant);

                if (statDice.DieNumber == 0 || statDice.DieSides == 0)
                    continue;

                var statRoll = statDice.Roll().PureSum;
                charStat.Moddifiers.Add($@"{stats.GroupTypeName}\StatRoll", statRoll);
            }

            return character;
        }

        public static Character AddTraits(this Character character, ITraitsContainer traits)
        {
            foreach (var trait in traits.Traits)
            {
                trait.Add(character);
            }

            return character;
        }

        public static Character AddSkills(this Character character, ISkillsContainer skills)
        {
            foreach (var skill in skills.Skills)
            {
                ESkillMastery charSkill;

                if (!character.Skills.TryGetValue(skill.Key, out charSkill)
                    || skill.Value > charSkill)
                    character.Skills[skill.Key] = skill.Value;
            }

            return character;
        }

        public static Character AddFreebies(this Character character, IFreebiesContainer freebies)
        {
            foreach (var freebie in freebies.Freebies)
            {
                character.Freebies.Add(freebie);
            }

            return character;
        }

        public static Character RerollStat(this Character character, ECharacterStat stat)
        {
            var charStat = character.Stats[stat];

            var roll = (IntAddModdifier)charStat.Moddifiers[$@"{Race.GroupType}\StatRoll"];
#warning вынести ролл 2d10 куда-то из рерола
            roll.Value = new Dice(2, 10).Roll();

            return character;
        }

        private static IDictionary<ECreationStage, ECreationStage> _creationStageChanges = new Dictionary<ECreationStage, ECreationStage>
        {
            [ECreationStage.RaceSelection] = ECreationStage.StatReroll,
            [ECreationStage.StatReroll] = ECreationStage.WorldSelection,
            [ECreationStage.WorldSelection] = ECreationStage.ClassSelection,

            [ECreationStage.ClassSelection] = ECreationStage.Finished,
        };

        public static bool TryIncraseCreationStage(this Character character, ECreationStage requiredStage)
        {
            ECreationStage newStage;

            if (character.CreationStage != requiredStage
                || !_creationStageChanges.TryGetValue(requiredStage, out newStage))
                return false;

            character.CreationStage = newStage;

            return true;
        }
    }
}
