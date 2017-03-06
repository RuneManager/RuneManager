#pragma once

#include "Stats.h"

class Monster;

namespace RUNESET
{
	enum RUNELIB_API RuneSet
	{
		Unknown = -1, // SW Proxy say what?

		Null, // No set

		Energy, // Health
		Guard, // Def
		Swift, // Speed
		Blade, // CRate
		Rage, // CDmg
		Focus, // Acc
		Endure, // Res
		Fatal, // Attack

		__unknown9,

		// Here be magic
		Despair,
		Vampire,

		__unknown12,

		Violent,
		Nemesis,
		Will,
		Shield,
		Revenge,
		Destroy,

		// Ally sets
		Fight,
		Determination,
		Enhance,
		Accuracy,
		Tolerance,

		Broken
	};
}

class RUNELIB_API Rune
{
public:
	Rune();
	~Rune();

	int ID;
	RUNESET::RuneSet Set;
	int Grade;
	int Slot;
	int Level;
	bool Locked;
	int AssignedId;
	char* AssignedName;

	STATATTR::Attr InnateType;
	int InnateValue;
	STATATTR::Attr MainType;
	int MainValue;
	STATATTR::Attr Sub1Type;
	int Sub1Value;
	STATATTR::Attr Sub2Type;
	int Sub2Value;
	STATATTR::Attr Sub3Type;
	int Sub3Value;
	STATATTR::Attr Sub4Type;
	int Sub4Value;

	//int* zero;
	int* healthPercent;
	int* attackPercent;
	int* defensePercent;
	int* speed;
	int* critRate;
	int* critDamage;
	int* accuracy;
	int* resistance;
	
	int* healthFlat;
	int* attackFlat;
	int* defenseFlat;
	int* speedPercent;

	Monster* Assigned;
	bool Swapped;
	static const int Rune::UnequipCosts[];
	bool SetIs4()
	{
		if (setIs4 == 3) setIs4 = (Rune::SetRequired(this->Set) == 4);
		return (setIs4 == 1);
	}

	int Rarity()
	{
		if (Sub1Type == STATATTR::Null) return 0; // Normal
		if (Sub1Type == STATATTR::Null) return 1; // Magic
		if (Sub1Type == STATATTR::Null) return 2; // Rare
		if (Sub1Type == STATATTR::Null) return 3; // Hero
		return 4; // Legend
	}

	int GetValue(STATATTR::Attr stat, int FakeLevel = -1, bool PredictSubs = false);
	bool HasStat(STATATTR::Attr stat, int fake = -1, bool pred = false);

	static int GetClassSize() { return sizeof(Rune); }

	Rune& SetValue(int p, STATATTR::Attr a, int v);

	static const int MainValues[11][4][16];

	static int SetRequired(RUNESET::RuneSet set)
	{
		if (set == RUNESET::Swift ||
			set == RUNESET::Fatal ||
			set == RUNESET::Violent ||
			set == RUNESET::Vampire ||
			set == RUNESET::Despair ||
			set == RUNESET::Rage
			)
			return 4;
		return 2;
	}

private:
	int setIs4;

};

