using Holism.Social.Business;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Holism.Social.UserApi.Controllers
{
    public class CommentController : DefaultController
    {
        string socialDatabaseName;

        string entityDatabaseName;

        public CommentController(string socialDatabaseName = null, string entityDatabaseName = null)
        {
            this.socialDatabaseName = socialDatabaseName;
            this.entityDatabaseName = entityDatabaseName;
        }

        [HttpPost]
        public IActionResult ToggleLike(Guid commentGuid)
        {
            new LikeBusiness(socialDatabaseName, entityDatabaseName).ToggleLike(UserGuid, CommentBusiness.EntityType, commentGuid);
            return OkJson();
        }

        [HttpPost]
        public IActionResult ToggleDislike(Guid commentGuid)
        {
            new DislikeBusiness(socialDatabaseName, entityDatabaseName).ToggleDislike(UserGuid, CommentBusiness.EntityType, commentGuid);
            return OkJson();
        }

        [HttpPost]
        public IActionResult ToggleApprovedState(long id)
        {
            ((CommentBusiness)Business).ToggleApprovedState(id);
            return OkJson();
        }

        [HttpPost]
        public IActionResult ApproveItems(List<long> ids)
        {
            ((CommentBusiness)Business).ApproveItems(ids);
            return OkJson();
        }

        [HttpPost]
        public IActionResult DisapproveItems(List<long> ids)
        {
            ((CommentBusiness)Business).DisapproveItems(ids);
            return OkJson();
        }
    }
}
