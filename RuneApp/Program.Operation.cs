using System.Linq;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;

namespace RuneApp
{
    public static partial class Program
    {

        public static void AddMonster(Monster mon)
        {
            if (data.Monsters.Any(m => m.Id == mon.Id))
            {
                UpdateMonster(mon);
                return;
            }

            if (mon.WizardId == data.WizardInfo.Id)
            {
                data.Monsters.Add(mon);
                data.isModified = true;
                OnMonsterUpdate?.Invoke(mon, false);
            }
        }

        public static void AddRune(Rune rune)
        {
            if (data.Runes.Any(r => r.Id == rune.Id))
            {
                UpdateRune(rune);
                return;
            }

            if (rune.WizardId == data.WizardInfo.Id)
            {
                data.Runes.Add(rune);
                data.isModified = true;
                OnRuneUpdate?.Invoke(rune, false);
            }
        }

        public static void DeleteMonster(Monster mon)
        {
            var m = data.GetMonster(mon.Id);
            data.Monsters.Remove(m);

            data.isModified = true;
            OnMonsterUpdate?.Invoke(mon, true);
        }

        public static void DeleteRune(Rune rune)
        {
            var r = data.GetRune(rune.Id);
            data.Runes.Remove(r);

            data.isModified = true;
            OnRuneUpdate?.Invoke(rune, true);
        }

        public static void UpdateMonster(Monster mon)
        {
            var m = data.GetMonster(mon.Id);
            if (m == null)
            {
                AddMonster(mon);
                return;
            }

            // TODO: modify stats and trigger callbacks
            m.level = mon.level;
            m.Grade = mon.Grade;
            m.monsterTypeId = mon.monsterTypeId;

            foreach (var attr in Build.StatEnums)
            {
                m[attr] = mon[attr];
            }

            for (int i = 0; i < mon._SkillList.Count; i++)
            {
                m._SkillList[i].Level = mon._SkillList[i].Level;
            }
            m.damageFormula = null;
            m.RefreshStats();

            for (int i = 0; i < 6; i++)
            {
                // find the changes per slot
                var rl = mon.Runes.FirstOrDefault(r => r.Slot - 1 == i);
                if (rl != null)
                {
                    var rune = data.GetRune(rl.Id);
                    if (rune != null)
                    {
                        // if the new rune is assigned to someone else
                        if (rune.AssignedId != m.Id && rune.AssignedId > 0)
                        {
                            var om = data.GetMonster(rune.AssignedId);
                            var rm = om?.RemoveRune(rune.Slot);
                            if (rm != null)
                            {
                                rm.Assigned = null;
                                rm.AssignedId = 0;
                                rm.AssignedName = "Unassigned";
                            }
                        }
                        // unassign any runes that point to this slot (just to be safe)
                        /*foreach (var r in data.Runes.Where(r => r.AssignedId == m.Id && r.Slot - 1 == i)) {
                            r.Assigned = null;
                            r.AssignedId = 0;
                            r.AssignedName = "Unassigned";
                        }*/
                        // assign the new rune to the current monster
                        rune.AssignedId = m.Id;
                        rune.Assigned = m;
                        rune.AssignedName = m.FullName;
                        if (rune != m.Current.Runes[i])
                        {
                            var rm = m.ApplyRune(rune);
                            if (rm != null)
                            {
                                rm.Assigned = null;
                                rm.AssignedId = 0;
                                rm.AssignedName = "Unassigned";
                            }
                        }
                    }
                }
                else
                {
                    // pull a rune off, if that don't work find anyrune who was pointing at my slot
                    var rm = m.RemoveRune(i + 1) ?? data.Runes.FirstOrDefault(r => r.AssignedId == m.Id && r.Slot - 1 == i);
                    if (rm != null)
                    {
                        rm.Assigned = null;
                        rm.AssignedId = 0;
                        rm.AssignedName = "Unassigned";
                    }
                }
            }

            data.isModified = true;
            OnMonsterUpdate?.Invoke(m, false);
        }

        public static void UpdateRune(Rune rune, bool keepLocked = true, Monster newAssigned = null)
        {
            var r = data.GetRune(rune.Id);
            if (r == null)
                return;
            // TODO: modify stats and trigger callbacks
            rune.CopyTo(r, keepLocked, newAssigned);

            //r.Assigned?.Current.AddRune(r);

            data.isModified = true;
            OnRuneUpdate?.Invoke(r, false);
        }

    }
}
