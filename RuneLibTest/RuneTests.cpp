#include "stdafx.h"
#include "CppUnitTest.h"

#include "..\RuneLib\Rune.h"
#include "TestData.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace RuneLibTest
{		
	TEST_CLASS(RuneTests)
	{
	public:

		TEST_METHOD(SetValue)
		{
			Rune r;
			r.Level = 6;
			r.Grade = 5;
			r.Slot = 2;
			r.Set = RUNESET::Swift;
			r.SetValue(-1, STATATTR::Resistance, 3);
			r.SetValue(0, STATATTR::AttackPercent, 22);
			r.SetValue(1, STATATTR::CritRate, 9);
			r.SetValue(2, STATATTR::Speed, 5);

			Assert::AreEqual(22, r.attackPercent[0]);
			Assert::AreEqual(r.attackPercent[0], r.GetValue(STATATTR::AttackPercent));
			Assert::AreEqual(r.attackPercent[12 + 16], r.GetValue(STATATTR::AttackPercent, 12, true));
			Assert::AreEqual(r.attackPercent[15], r.GetValue(STATATTR::AttackPercent, 15));

			Assert::AreEqual(0, r.healthPercent[0]);
			Assert::AreEqual(r.GetValue(STATATTR::HealthPercent), r.healthPercent[0]);
			Assert::AreEqual(1, r.healthPercent[12 + 16]);
			Assert::AreEqual(r.GetValue(STATATTR::HealthPercent, 12, true), r.healthPercent[12 + 16]);
			Assert::AreEqual(0, r.healthPercent[15]);
			Assert::AreEqual(r.GetValue(STATATTR::HealthPercent, 15), r.healthPercent[15]);
		}

		TEST_METHOD(SetRequiredTest)
		{
			Assert::AreEqual(2, Rune::SetRequired(TestData::Rune1().Set));
			Assert::AreEqual(4, Rune::SetRequired(TestData::Rune2().Set));
		}

		TEST_METHOD(GetValueTest)
		{
			Rune rune = TestData::Rune1();
			Assert::AreEqual(78, rune.GetValue(STATATTR::AttackFlat));
			Assert::AreEqual(99, rune.GetValue(STATATTR::AttackFlat, 12, true));
			Assert::AreEqual(135, rune.GetValue(STATATTR::AttackFlat, 15));

			Assert::AreEqual(0, rune.GetValue(STATATTR::HealthFlat));
			Assert::AreEqual(1, rune.GetValue(STATATTR::HealthFlat, 12, true));
			Assert::AreEqual(0, rune.GetValue(STATATTR::HealthFlat, 15));
		}

		TEST_METHOD(HasStatTest)
		{
			Rune rune = TestData::Rune1();
			Assert::IsTrue(rune.HasStat(STATATTR::Speed));
			Assert::IsTrue(rune.HasStat(STATATTR::Speed, 12, true));
			Assert::IsTrue(rune.HasStat(STATATTR::Speed, 15));

			Assert::IsFalse(rune.HasStat(STATATTR::HealthPercent));
			Assert::IsTrue(rune.HasStat(STATATTR::HealthPercent, 12, true));
			Assert::IsFalse(rune.HasStat(STATATTR::HealthPercent, 15, false));
		}

	};
}