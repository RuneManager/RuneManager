#pragma once

#include "RuneLib.h"

namespace STATATTR
{
	enum RUNELIB_API Attr
	{
		Neg = -2,
		Null = -1,
		HealthPercent = 0,
		AttackPercent = 1,
		DefensePercent = 2,
		Speed = 3,
		CritRate = 4,
		CritDamage = 5,
		Resistance = 6,
		Accuracy = 7,

		// superfast hax
		HealthFlat = 8,
		AttackFlat = 9,
		DefenseFlat = 10,
		// Thanks Swift -_-
		SpeedPercent = 11,

		ExtraStat = 16,
		EffectiveHP = 1 | ExtraStat,
		EffectiveHPDefenseBreak = 2 | ExtraStat,
		DamagePerSpeed = 3 | ExtraStat,
		AverageDamage = 4 | ExtraStat,
		MaxDamage = 5 | ExtraStat
	};
}

class RUNELIB_API Stats
{
public:
	Stats();
	~Stats();

	double Health;
	double Attack;
	double Defense;
	double Speed;
	double CritRate;
	double CritDamage;
	double Resistance;
	double Accuracy;

	double operator[](STATATTR::Attr a);

};

