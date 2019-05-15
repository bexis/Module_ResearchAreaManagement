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

                Feature rootResearchAreaFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Research Area Management"));
                if (rootResearchAreaFeature == null) rootResearchAreaFeature = featureManager.Create("Research Area Management", "Research Area Management");


                Feature researchAreasFeature = features.FirstOrDefault(f => f.Name.Equals("Research Areas"));
                if (researchAreasFeature == null)
                    researchAreasFeature = featureManager.Create("Research Areas", "Research Areas", rootResearchAreaFeature);

                Feature researchAreasAdminFeature = features.FirstOrDefault(f => f.Name.Equals("Research Areas Admin"));
                if (researchAreasAdminFeature == null)
                    researchAreasAdminFeature = featureManager.Create("Research Areas Admin", "Research Areas Admin", rootResearchAreaFeature);


                operationManager.Create("PMM", "Main", "*", researchAreasFeature);

                operationManager.Create("PMM", "MainAdmin", "*", researchAreasAdminFeature);

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