using System;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace MountAndWarcraftReborn.Hirelings
{
    public class MWRHirelingActivities
    {
        private readonly Dictionary<string, List<MWRHirelingActivityDefinition>> _activitySets =
            new Dictionary<string, List<MWRHirelingActivityDefinition>>(StringComparer.OrdinalIgnoreCase)
            {
                ["empire"] = new List<MWRHirelingActivityDefinition>
                {
                    new MWRHirelingActivityDefinition("{=mwr_hireling_emp_0}Drill with the infantry", "{=mwr_hireling_emp_0_tip}Practice close-order fighting with the line troops.", DefaultSkills.OneHanded),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_emp_1}Practice with missile troops", "{=mwr_hireling_emp_1_tip}Train alongside archers and crossbowmen.", DefaultSkills.Bow),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_emp_2}Study battlefield plans", "{=mwr_hireling_emp_2_tip}Observe officers and learn from their deployments.", DefaultSkills.Tactics),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_emp_3}Help the quartermaster", "{=mwr_hireling_emp_3_tip}Assist with supply lists, stores, and camp logistics.", DefaultSkills.Steward),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_emp_4}Ride ahead with the scouts", "{=mwr_hireling_emp_4_tip}Range ahead of the host and report movement.", DefaultSkills.Scouting),
                },
                ["vlandia"] = new List<MWRHirelingActivityDefinition>
                {
                    new MWRHirelingActivityDefinition("{=mwr_hireling_vla_0}Spar with sword and shield", "{=mwr_hireling_vla_0_tip}Refine your technique among hardened men-at-arms.", DefaultSkills.OneHanded),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_vla_1}Practice with the lance", "{=mwr_hireling_vla_1_tip}Join the cavalry yard and train your reach.", DefaultSkills.Polearm),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_vla_2}Care for your mount", "{=mwr_hireling_vla_2_tip}Ride, groom, and condition your horse.", DefaultSkills.Riding),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_vla_3}Observe the knights", "{=mwr_hireling_vla_3_tip}Learn the habits of nobles and camp commanders.", DefaultSkills.Leadership),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_vla_4}Oversee camp stores", "{=mwr_hireling_vla_4_tip}Track equipment, rations, and repair needs.", DefaultSkills.Steward),
                },
                ["battania"] = new List<MWRHirelingActivityDefinition>
                {
                    new MWRHirelingActivityDefinition("{=mwr_hireling_bat_0}Practice with the longbow", "{=mwr_hireling_bat_0_tip}Loose arrows until the fingers ache.", DefaultSkills.Bow),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_bat_1}Scout the woods", "{=mwr_hireling_bat_1_tip}Range through rough country unseen.", DefaultSkills.Scouting),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_bat_2}Train in skirmishing", "{=mwr_hireling_bat_2_tip}Move fast, strike hard, and fade away.", DefaultSkills.Athletics),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_bat_3}Sharpen your javelin work", "{=mwr_hireling_bat_3_tip}Drill your throwing arm with the kern.", DefaultSkills.Throwing),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_bat_4}Gather herbs for the wounded", "{=mwr_hireling_bat_4_tip}Learn field remedies from seasoned camp followers.", DefaultSkills.Medicine),
                },
                ["sturgia"] = new List<MWRHirelingActivityDefinition>
                {
                    new MWRHirelingActivityDefinition("{=mwr_hireling_stu_0}Train with axe and shield", "{=mwr_hireling_stu_0_tip}Harden yourself in brutal shieldwall drills.", DefaultSkills.OneHanded),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_stu_1}Practice heavy strikes", "{=mwr_hireling_stu_1_tip}Build power with two-handed weapons.", DefaultSkills.TwoHanded),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_stu_2}Range across the wilds", "{=mwr_hireling_stu_2_tip}Track movement through snow and forest.", DefaultSkills.Scouting),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_stu_3}Toughen your body", "{=mwr_hireling_stu_3_tip}Run drills and endure the cold with the veterans.", DefaultSkills.Athletics),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_stu_4}Share fire and war stories", "{=mwr_hireling_stu_4_tip}Earn standing among the warriors of the host.", DefaultSkills.Leadership),
                },
                ["khuzait"] = new List<MWRHirelingActivityDefinition>
                {
                    new MWRHirelingActivityDefinition("{=mwr_hireling_khu_0}Ride with the outriders", "{=mwr_hireling_khu_0_tip}Spend the day in the saddle.", DefaultSkills.Riding),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_khu_1}Practice horse archery", "{=mwr_hireling_khu_1_tip}Loose arrows while keeping your seat.", DefaultSkills.Bow),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_khu_2}Sweep the horizon", "{=mwr_hireling_khu_2_tip}Keep watch for movement across the steppe.", DefaultSkills.Scouting),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_khu_3}Drill with the spear", "{=mwr_hireling_khu_3_tip}Sharpen your reach from horseback and foot.", DefaultSkills.Polearm),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_khu_4}Listen to the captains", "{=mwr_hireling_khu_4_tip}Absorb the rhythm of mobile warfare.", DefaultSkills.Tactics),
                },
                ["aserai"] = new List<MWRHirelingActivityDefinition>
                {
                    new MWRHirelingActivityDefinition("{=mwr_hireling_ase_0}Ride with the caravan guards", "{=mwr_hireling_ase_0_tip}Learn the routes and rhythms of desert travel.", DefaultSkills.Riding),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_ase_1}Watch the dunes", "{=mwr_hireling_ase_1_tip}Study distant movement and protect the column.", DefaultSkills.Scouting),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_ase_2}Drill with bow and javelin", "{=mwr_hireling_ase_2_tip}Train for skirmish and pursuit.", DefaultSkills.Bow),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_ase_3}Haggle with camp traders", "{=mwr_hireling_ase_3_tip}Help secure prices and supplies.", DefaultSkills.Trade),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_ase_4}Help run the camp", "{=mwr_hireling_ase_4_tip}Support the officers and logistics train.", DefaultSkills.Steward),
                },
                ["nord"] = new List<MWRHirelingActivityDefinition>
                {
                    new MWRHirelingActivityDefinition("{=mwr_hireling_nord_0}Sharpen dead blades", "{=mwr_hireling_nord_0_tip}Tend ancient weapons in grim silence.", DefaultSkills.OneHanded),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_nord_1}Drill with relentless strikes", "{=mwr_hireling_nord_1_tip}Repeat heavy killing blows without rest.", DefaultSkills.TwoHanded),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_nord_2}Patrol the cold perimeter", "{=mwr_hireling_nord_2_tip}Watch the edges of camp for prey and intruders.", DefaultSkills.Scouting),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_nord_3}March with the dead", "{=mwr_hireling_nord_3_tip}Learn to keep the host ordered in silence.", DefaultSkills.Leadership),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_nord_4}Study the slaughter", "{=mwr_hireling_nord_4_tip}Observe how terror and attrition break the foe.", DefaultSkills.Tactics),
                },
                ["default"] = new List<MWRHirelingActivityDefinition>
                {
                    new MWRHirelingActivityDefinition("{=mwr_hireling_def_0}Train with the line troops", "{=mwr_hireling_def_0_tip}Practice basic weapons and shield work.", DefaultSkills.OneHanded),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_def_1}Scout the surrounding land", "{=mwr_hireling_def_1_tip}Ride or march ahead looking for danger.", DefaultSkills.Scouting),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_def_2}Study the officers", "{=mwr_hireling_def_2_tip}Watch how command decisions are made.", DefaultSkills.Tactics),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_def_3}Assist camp logistics", "{=mwr_hireling_def_3_tip}Help move supplies and keep order.", DefaultSkills.Steward),
                    new MWRHirelingActivityDefinition("{=mwr_hireling_def_4}Tend to the wounded", "{=mwr_hireling_def_4_tip}Learn the routines of battlefield care.", DefaultSkills.Medicine),
                }
            };

        public List<MWRHirelingActivityDefinition> GetActivities(string cultureId)
        {
            if (!string.IsNullOrEmpty(cultureId) && _activitySets.TryGetValue(cultureId, out List<MWRHirelingActivityDefinition> activities))
            {
                return activities;
            }

            return _activitySets["default"];
        }
    }
}
