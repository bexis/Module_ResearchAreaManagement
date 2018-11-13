using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Pmm.UI.Helper
{
    public class PmmSeedDataGenerator : IDisposable
    {
        public void Dispose()
        {
            //throw new NotImplementedException();
        }


        public void GenerateSeedData()
        {
            #region SECURITY

            OperationManager operationManager = new OperationManager();
            FeatureManager featureManager = new FeatureManager();

            try {

                List<Feature> features = featureManager.FeatureRepository.Get().ToList();

                Feature ResearchAreas = features.FirstOrDefault(f => f.Name.Equals("Research Areas"));
                if (ResearchAreas == null)
                    ResearchAreas = featureManager.Create("Research Areas", "Research Areas");

                Feature ResearchAreasAdmin = features.FirstOrDefault(f => f.Name.Equals("Research Areas Admin"));
                if (ResearchAreasAdmin == null)
                    ResearchAreasAdmin = featureManager.Create("Research Areas Admin", "Research Areas Admin");


                operationManager.Create("PMM", "Main", "*", ResearchAreas);

                operationManager.Create("PMM", "MainAdmin", "*", ResearchAreasAdmin);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {
                operationManager.Dispose();
                featureManager.Dispose();

            }
        
        }
            #endregion
    }
}