#pragma once

#include "..\RuneLib\Rune.h"

namespace TestData
{
	static Rune Rune1()
	{
		Rune r;
		r.Level = 9;
		r.Grade = 5;
		r.Slot = 1;
		r.Set = RUNESET::Energy;
		r.MainType = STATATTR::AttackFlat;
		r.MainValue = 78;
		r.InnateType = STATATTR::Null;
		r.InnateValue = 0;
		r.Sub1Type = STATATTR::AttackPercent;
		r.Sub1Value = 18;
		r.Sub2Type = STATATTR::Speed;
		r.Sub2Value = 4;
		r.Sub3Type = STATATTR::CritRate;
		r.Sub3Value = 4;
		r.Sub4Type = STATATTR::Null;
		r.Sub4Value = 0;
		return r;
	}

	static Rune Rune2()
	{
		Rune r;
		r.Level = 6;
		r.Grade = 5;
		r.Slot = 2;
		r.Set = RUNESET::Swift;
		r.MainType = STATATTR::AttackPercent;
		r.MainValue = 22;
		r.InnateType = STATATTR::Resistance;
		r.InnateValue = 3;
		r.Sub1Type = STATATTR::CritRate;
		r.Sub1Value = 9;
		r.Sub2Type = STATATTR::Speed;
		r.Sub2Value = 5;
		r.Sub3Type = STATATTR::Null;
		r.Sub3Value = 0;
		r.Sub4Type = STATATTR::Null;
		r.Sub4Value = 0;
		return r;
	}
}