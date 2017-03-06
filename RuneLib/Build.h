#pragma once

#include "Stats.h"

class RUNELIB_API Build
{
public:
	Build();
	~Build();
	static const STATATTR::Attr statAll[];
	double sort(Stats & m);
	Stats Sort;
};
