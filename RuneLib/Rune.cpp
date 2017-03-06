#include "Rune.h"
#include "Monster.h"

const int Rune::UnequipCosts[] = { 1000, 2500, 5000, 10000, 25000, 50000 };

Rune::Rune()
{
	AssignedId = 0;
	AssignedName = nullptr;
	ID = 0;
	Set = RUNESET::Null;
	Grade = 0;
	Slot = 0;
	Level = 0;
	Locked = false;

	static int zero[32] = { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };

	healthPercent = zero;
	attackPercent = zero;
	defensePercent = zero;
	speed = zero;
	critRate = zero;
	critDamage = zero;
	accuracy = zero;
	resistance = zero;

	healthFlat = zero;
	attackFlat = zero;
	defenseFlat = zero;
	speedPercent = zero;

	Assigned = nullptr;
	Swapped = false;

	setIs4 = 3;
}


Rune::~Rune()
{
}

int min(int a, int b)
{
	return a > b ? b : a;
}
int max(int a, int b)
{
	return a > b ? a : b;
}

bool Rune::HasStat(STATATTR::Attr stat, int fake, bool pred)
{
	if (GetValue(stat, fake, pred) > 0)
		return true;
	return false;
}

int Rune::GetValue(STATATTR::Attr stat, int FakeLevel, bool PredictSubs)
{
	if (MainType == stat)
	{
		if (FakeLevel <= Level || FakeLevel > 15 || Grade < 3)
		{
			return MainValue;
		}
		else
		{
			return MainValues[MainType][Grade - 3][FakeLevel];
		}
	}
	if (InnateType == stat) return InnateValue;
	if (PredictSubs == false)
	{
		if (Sub1Type == stat || Sub1Type == STATATTR::Null) return Sub1Value;
		if (Sub2Type == stat || Sub2Type == STATATTR::Null) return Sub2Value;
		if (Sub3Type == stat || Sub3Type == STATATTR::Null) return Sub3Value;
		if (Sub4Type == stat || Sub4Type == STATATTR::Null) return Sub4Value;
	}
	else
	{
		// count how many upgrades have gone into the rune
		int maxUpgrades = min(Rarity(), max(Level, FakeLevel) / 3);
		int upgradesGone = min(4, Level / 3);
		// how many new sub are to appear (0 legend will be 4 - 4 = 0, 6 rare will be 4 - 3 = 1, 6 magic will be 4 - 2 = 2)
		int subNew = 4 - Rarity();
		// how many subs will go into existing stats (0 legend will be 4 - 0 - 0 = 4, 6 rare will be 4 - 1 - 2 = 1, 6 magic will be 4 - 2 - 2 = 0)
		int subEx = maxUpgrades - upgradesGone;// - subNew;
		int subVal = (subNew > 0 ? 1 : 0);

		if (Sub1Type == stat || Sub1Type == STATATTR::Null) return (Sub1Value + subEx);
		if (Sub2Type == stat || Sub2Type == STATATTR::Null) return (Sub2Value + subEx);
		if (Sub3Type == stat || Sub3Type == STATATTR::Null) return (Sub3Value + subEx);
		if (Sub4Type == stat || Sub4Type == STATATTR::Null) return (Sub4Value + subEx);
	}

	return 0;
}

unsigned int Log2(unsigned int v)
{
	static const int MultiplyDeBruijnBitPosition[32] =
	{
		0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
		8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
	};

	v |= v >> 1; // first round down to one less than a power of 2 
	v |= v >> 2;
	v |= v >> 4;
	v |= v >> 8;
	v |= v >> 16;

	return MultiplyDeBruijnBitPosition[(unsigned int)(v * 0x07C4ACDDU) >> 27];
}

Rune& Rune::SetValue(int p, STATATTR::Attr a, int v)
{
#ifdef _DEBUG
	if (a < -1) throw "):";
	if (a > 15) throw ":("; // unsure if okay
#else
	if (a > 15) return *this;
#endif
	*(&MainType + p * 2) = a;
	*(&MainValue + p * 2) = v;
	*(&healthFlat + a) = &MainValue + p * 2;
	return *this;
}


#define MAINVALUES_FLAT {\
{7,12,17,22,27,32,37,42,47,52,57,62,67,72,77,92},\
{10,16,22,28,34,40,46,52,58,64,70,76,82,88,94,112},\
{15,22,29,36,43,50,57,64,71,78,85,92,99,106,113,135},\
{22,30,38,46,54,62,70,78,86,94,102,110,118,126,134,160}\
}

#define MAINVALUES_PERCENT {\
{4,6,8,10,12,14,16,18,20,22,24,26,28,30,32,38},\
{5, 7, 9, 11, 13, 16, 18, 20, 22, 23, 27, 29, 31, 33, 36, 43},\
{8, 10, 12, 15, 17, 20, 22, 24, 27, 29, 32, 34, 37, 40, 43, 51},\
{11, 14, 17, 20, 23, 26, 29, 32, 35, 38, 41, 44, 47, 50, 53, 63}\
}

#define MAINVALUES_RESACC {\
{4,6,8,10,12,14,16,18,20,22,24,26,28,30,32,38},\
{6, 8, 10, 13, 15, 17, 19, 21, 24, 26, 28, 30, 32, 35, 37, 44},\
{9, 11, 14, 16, 19, 21, 23, 26, 28, 31, 33, 35, 38, 40, 43, 51},\
{12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 64}\
}

const int Rune::MainValues[11][4][16] = {
	// Health Percent
	MAINVALUES_PERCENT,
	// Attack Percent
	MAINVALUES_PERCENT,
	// Defense Percent
	MAINVALUES_PERCENT,
	// Speed
	{ { 3,4,5,6,8,9,10,12,13,14,16,17,18,19,21,25 },
	{ 4,5,7,8,10,11,13,14,16,17,19,20,22,23,25,30 },
	{ 5,7,9,11,13,15,17,19,21,23,25,27,29,31,33,39 },
	{ 7,9,11,13,15,17,19,21,23,25,27,29,31,33,35,42 }
	},
	// Crit Rate
	{ { 3,5,7,9,11,13,15,17,19,21,23,25,27,29,31,37 },
	{ 4,6,8,11,13,15,17,19,22,24,26,28,30,33,35,41 },
	{ 5,7,10,12,15,17,19,22,24,27,29,31,34,36,39,47 },
	{ 7,10,13,16,19,22,25,28,31,34,37,40,43,46,49,58 }
	},
	// Crit Damage
	{ { 4,6,9,11,13,16,18,20,22,25,27,29,32,34,36,43 },
	{ 6,9,12,15,18,21,24,27,30,33,36,39,42,45,48,57 },
	{ 8,11,15,18,21,25,28,31,34,38,41,44,48,51,54,65 },
	{ 11,15,19,23,27,31,35,39,43,47,51,55,59,63,67,80 }
	},
	// Accuracy
	MAINVALUES_RESACC,
	// Resistance
	MAINVALUES_RESACC,
	// Health Flat
	{ { 100,175,250,325,400,475,550,625,700,775,850,925,1000,1075,1150,1380 },
	{ 160,250,340,430,520,610,700,790,880,970,1060,1150,1240,1330,1420,1704 },
	{ 270,375,480,585,690,795,900,1005,1110,1215,1320,1425,1530,1635,1740,2088 },
	{ 360,480,600,720,840,960,1080,1200,1320,1440,1560,1680,1800,1920,2040,2448 }
	},
	// Attack Flat
	MAINVALUES_FLAT,
	// Defense Flat
	MAINVALUES_FLAT,
};
