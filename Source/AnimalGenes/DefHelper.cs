using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes
{
    public class DefHelper
    {
        public static void CopyHediffDefFields(HediffDef sThing, HediffDef newThing)
        {
            foreach (var field in sThing.GetType().GetFields().Where(x => !x.IsLiteral && !x.IsStatic))
            {
                try
                {
                    field.SetValue(newThing, field.GetValue(sThing));
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to copy field {field.Name} from HediffDef.");
                    Log.Error(e.ToString());
                }
            }
        }

        public static void CopyGeneDefFields(GeneDef sThing, GeneDef newThing)
        {
            foreach (var field in sThing.GetType().GetFields().Where(x => !x.IsLiteral && !x.IsStatic))
            {
                try
                {
                    field.SetValue(newThing, field.GetValue(sThing));
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to copy field {field.Name} from GeneDef.");
                    Log.Error(e.ToString());
                }
            }
        }

        public static void CopyRenderNodePropertiesDefFields(PawnRenderNodeProperties sThing, PawnRenderNodeProperties newThing)
        {
            foreach (var field in sThing.GetType().GetFields().Where(x => !x.IsLiteral && !x.IsStatic))
            {
                try
                {
                    field.SetValue(newThing, field.GetValue(sThing));
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to copy field {field.Name} from HediffDef.");
                    Log.Error(e.ToString());
                }
            }
        }
    }
}
