﻿using BExIS.Pmm.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;
using GeometryX = BExIS.Pmm.Entities.GeometryInformation;

namespace BExIS.Pmm.Services
{
    public sealed class GeometryManager
    {
        #region Attributes
        
        public IReadOnlyRepository<GeometryX> Repo { get; private set; }

        #endregion

        #region Ctors
        
        public GeometryManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.Repo = uow.GetReadOnlyRepository<GeometryX>();
        }

        #endregion
        
        #region Methods
        public GeometryX Create(long plotid, string coordinate, string geometrytype, string coordinatetype, string color, string geometrytext, Plot plot, string name, string description, string refrencePoint = "")
        {
            //Contract.Requires(!string.IsNullOrWhiteSpace(plotId));
            //Contract.Requires(!string.IsNullOrWhiteSpace(plotType) != null);
            //Contract.Requires(!string.IsNullOrWhiteSpace(latitude) != null);
            //Contract.Requires(!string.IsNullOrWhiteSpace(longitude) != null);
            Contract.Ensures(Contract.Result<GeometryX>() != null && Contract.Result<GeometryX>().Id >= 0);

            //PartyStatus initialStatus = new PartyStatus();
            //initialStatus.Timestamp = DateTime.UtcNow;
            //initialStatus.Description = "Created";
            //initialStatus.StatusType = statusType;

            GeometryX entity = new GeometryX()
            {
                Plot = plot,
                PlotId = plotid,
                GeometryText = geometrytext,
                GeometryType = geometrytype,
                Coordinate = coordinate,
                CoordinateType = coordinatetype,
                Color = color,
                Status = 1,
                LineWidth = 1,
                Name = name,
                Description = description,
                GeometryId = 1,
                VersionNo = 1,
                Extra = null,
                RefrencePoint = refrencePoint
            };
            //entity.History.Add(initialStatus);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<GeometryX> repo = uow.GetRepository<GeometryX>();
                repo.Put(entity); // must store the status objects too
                uow.Commit();
            }
            return (entity);
        }

        public bool Delete(GeometryX entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<GeometryX> repo = uow.GetRepository<GeometryX>();
                
                entity = repo.Reload(entity);

                
                //delete the entity
                repo.Delete(entity);

                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<GeometryX> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (GeometryX e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (GeometryX e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<GeometryX> repo = uow.GetRepository<GeometryX>();
                
                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);

                    //delete the unit
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
        }

        public GeometryX Update(GeometryX entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<GeometryX>() != null && Contract.Result<GeometryX>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<GeometryX> repo = uow.GetRepository<GeometryX>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }


        #endregion

              
        #region Associations
        #endregion
    }
}
