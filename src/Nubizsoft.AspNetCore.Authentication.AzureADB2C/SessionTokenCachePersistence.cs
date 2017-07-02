using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;

namespace Nubizsoft.AspNetCore.Authentication.AzureADB2C
{
    public class SessionTokenCachePersistence : BaseTokenCachePersistence
    {
        private readonly string _userId;
        private readonly string _cacheId;
        private readonly HttpContext _httpContext;

        public SessionTokenCachePersistence(string userId, HttpContext httpcontext)
        {
            _userId = userId;
            _cacheId = _userId + "_TokenCache";
            _httpContext = httpcontext;
        }

        public override void Load()
        {
			TokenCacheLock.EnterReadLock();
			UsertokenCache.Deserialize(_httpContext.Session.Get(_cacheId));
			TokenCacheLock.ExitReadLock();
        }

        public override void Persist()
        {
			TokenCacheLock.EnterWriteLock();

			UsertokenCache.HasStateChanged = false;

			_httpContext.Session.Set(_cacheId, UsertokenCache.Serialize());
			TokenCacheLock.ExitWriteLock();
        }

        public override string ReadUserStateValue()
        {
			string state = string.Empty;
			TokenCacheLock.EnterReadLock();
			state = (string)_httpContext.Session.GetString(_cacheId + "_state");
			TokenCacheLock
                .ExitReadLock();
			return state;
        }

        public override void SaveUserStateValue(string state)
        {
			TokenCacheLock.EnterWriteLock();
			_httpContext.Session.SetString(_cacheId + "_state", state);
			TokenCacheLock.ExitWriteLock();
        }
    }
}
