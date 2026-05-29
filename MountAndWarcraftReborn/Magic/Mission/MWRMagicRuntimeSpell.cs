namespace MountAndWarcraftReborn.Magic.Mission
{
    public class MWRMagicRuntimeSpell
    {
        public MWRMagicRuntimeSpell(MWRMagicAbility ability)
        {
            Ability = ability;
        }

        public MWRMagicAbility Ability { get; }

        public MWRMagicSpellTemplate Template => Ability.Template;

        public float CooldownRemaining => Ability.CooldownRemaining;

        public bool IsReady => Ability.IsReady;

        public void TriggerCooldown()
        {
            Ability.StartCooldown();
        }
    }
}
