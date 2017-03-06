#pragma once

#include "Rune.h"
#include "Stats.h"

class Loadout
{
public:
	Loadout();
	~Loadout();

	Rune* runes[6];
	int runeCount;
	RUNESET::RuneSet sets[3];
	bool setsFull;

	int fakeLevel[6];
	bool predictSubs[6];

	int buildID;

	double Time;
	
	int runeIDs[6];

	Stats shrines;
	Stats leader;

	
};

