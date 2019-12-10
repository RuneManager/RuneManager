#include "Stats.h"

Stats::Stats()
{
}


Stats::~Stats()
{
}

double Stats::operator[](STATATTR::Attr a)
{
#ifdef _DEBUG
	if (a < 0) throw ":(";
	if (a > 11) throw ":(";
#endif
	return *(&Health + (int)a);
}
