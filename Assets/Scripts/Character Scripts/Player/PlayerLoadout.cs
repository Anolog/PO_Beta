
using System;

public enum BasicAttacks
{
    Melee,
    Ranged,
}

public enum DefensiveAbilites
{
    Charge,
}

public enum OtherAbilities
{
    RockThrow,
    GroundShock,
    None,
}

public enum AbilityTypes
{
    Basic,
    Defensive,
    Other,
}

public class PlayerLoadout
{
    private Controllers m_Controller;
    private BasicAttacks m_BasicAttack = BasicAttacks.Melee;
    private DefensiveAbilites m_DefensiveAbility = DefensiveAbilites.Charge;
    private OtherAbilities[] m_OtherAbilties = new OtherAbilities[3] { OtherAbilities.GroundShock, OtherAbilities.None, OtherAbilities.RockThrow};

    public Controllers controller { get { return m_Controller; }}

    public PlayerLoadout(Controllers aController)
    {
        m_Controller = aController;
    }

    public string GetAbilityName(byte aAbility)
    {
        if (aAbility == 0)
        {
            return m_BasicAttack.ToString();
        }
        else if (aAbility == 4)
        {
            return m_DefensiveAbility.ToString();
        }
        else
        {
            return m_OtherAbilties[aAbility - 1].ToString();
        }
    }

    public void NextAbility(byte aAbilityNumber)
    {
        if (aAbilityNumber == 0)
        {
            NextAbility(m_BasicAttack);
        }
        else if (aAbilityNumber == 4)
        {
            NextAbility(m_DefensiveAbility);
        }
        else
        {
            NextAbility(m_OtherAbilties[aAbilityNumber - 1]);
        }
    }

    public void NextAbility(BasicAttacks aBasic)
    {
        if ((int)aBasic++ > Enum.GetNames(typeof(BasicAttacks)).Length - 1)
        {
            aBasic = 0;
        }
        else
        {
            aBasic++;
        }
    }

    public void NextAbility(DefensiveAbilites aDefense)
    {
        if ((int)aDefense++ > Enum.GetNames(typeof(DefensiveAbilites)).Length - 1)
        {
            aDefense = 0;
        }
        else
        {
            aDefense++;
        }
    }

    public void NextAbility(OtherAbilities aAbility)
    {
        if ((int)aAbility++ > Enum.GetNames(typeof(OtherAbilities)).Length - 1)
        {
            aAbility = 0;
        }
        else
        {
            aAbility++;
        }
    }

    public void PreviousAbility(byte aAbilityNumber)
    {
        if (aAbilityNumber == 0)
        {
            PreviousAbility(m_BasicAttack);
        }
        else if (aAbilityNumber == 4)
        {
            PreviousAbility(m_DefensiveAbility);
        }
        else
        {
            PreviousAbility(m_OtherAbilties[aAbilityNumber - 1]);
        }
    }

    public void PreviousAbility(BasicAttacks aBasic)
    {
        if ((int)aBasic-- < 0)
        {
            aBasic = (BasicAttacks)Enum.GetNames(typeof(BasicAttacks)).Length - 1;
        }
        else
        {
            aBasic--;
        }
    }

    public void PreviousAbility(DefensiveAbilites aDefense)
    {
        if ((int)aDefense-- < 0)
        {
            aDefense = (DefensiveAbilites)Enum.GetNames(typeof(DefensiveAbilites)).Length - 1;
        }
        else
        {
            aDefense--;
        }
    }

    public void PreviousAbility(OtherAbilities aAbility)
    {
        if ((int)aAbility-- < 0)
        {
            aAbility = (OtherAbilities)Enum.GetNames(typeof(OtherAbilities)).Length - 1;
        }
        else
        {
            aAbility--;
        }
    }

    public Ability[] GetAbilities(CharacterStats aCharacter)
    {
        Ability[] abilities = new Ability[5];

        abilities[0] = GetBasicAttack(m_BasicAttack, aCharacter);
        abilities[1] = GetOffensiveAbility(m_OtherAbilties[0], aCharacter);
        abilities[2] = GetOffensiveAbility(m_OtherAbilties[1], aCharacter);
        abilities[3] = GetOffensiveAbility(m_OtherAbilties[3], aCharacter);
        abilities[4] = GetDefensiveAbility(m_DefensiveAbility, aCharacter);

        return abilities;
    }

    private Ability GetBasicAttack(BasicAttacks aAttack, CharacterStats aCharacter)
    {
        Ability attack;

        if (aAttack == BasicAttacks.Melee)
        {
            attack = new BasicMeleeAbility(aCharacter);
        }
        else
        {
            attack = new BasicRangedAbility(aCharacter);
        }

        return attack;
    }

    private Ability GetDefensiveAbility(DefensiveAbilites aAbility, CharacterStats aCharacter)
    {
        Ability defensiveAbility;

        if (aAbility == DefensiveAbilites.Charge)
        {
            defensiveAbility = new ChargeAbility(aCharacter);
        }
        else
        {
            defensiveAbility = null;
        }

        return defensiveAbility;
    }

    private Ability GetOffensiveAbility(OtherAbilities aAbility, CharacterStats aCharacter)
    {
        Ability ability;

        if (aAbility == OtherAbilities.GroundShock)
        {
            ability = new GroundShockAbility(aCharacter);
        }
        else if (aAbility == OtherAbilities.RockThrow)
        {
            ability = new RockThrowAbility(aCharacter);
        }
        else
        {
            ability = null;
        }

        return ability;
    }
}
