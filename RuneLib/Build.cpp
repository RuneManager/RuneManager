#include "Build.h"

#include "Stats.h"

const STATATTR::Attr Build::statAll[] = { 
	STATATTR::HealthPercent, 
	STATATTR::AttackPercent, 
	STATATTR::DefensePercent, 
	STATATTR::Speed, 
	STATATTR::CritRate, 
	STATATTR::CritDamage, 
	STATATTR::Resistance, 
	STATATTR::Accuracy,
	STATATTR::EffectiveHP, 
	STATATTR::EffectiveHPDefenseBreak, 
	STATATTR::DamagePerSpeed, 
	STATATTR::AverageDamage, 
	STATATTR::MaxDamage 
};

Build::Build()
{
}


Build::~Build()
{
}

double Build::sort(Stats& m)
{
	double pts = 0;

	for each (auto stat in statAll)
	{
		if (stat && STATATTR::ExtraStat == STATATTR::ExtraStat)
		{

		}
		else
		{
			if (Sort[stat] != 0)
			{

			}
		}
	}

	return pts;
}