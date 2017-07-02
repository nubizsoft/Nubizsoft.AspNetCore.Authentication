using System;
using System.Threading;
using Microsoft.Identity.Client;

namespace Nubizsoft.AspNetCore.Authentication.AzureADB2C
{
    public abstract class BaseTokenCachePersistence
    {
        
        public TokenCache GetUserCache()
        {
            TokenCacheLock.EnterWriteLock();
            UsertokenCache.SetBeforeAccess(BeforeAccessNotification);
            UsertokenCache.SetAfterAccess(AfterAccessNotification);
            TokenCacheLock.ExitWriteLock();
            return UsertokenCache;
        }

		protected readonly TokenCache UsertokenCache = new TokenCache();

		protected static ReaderWriterLockSlim TokenCacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		public virtual void SaveUserStateValue(string state) => throw new NotImplementedException();
		
        public virtual string ReadUserStateValue() => throw new NotImplementedException();

        public virtual void Load() => throw new NotImplementedException();

		public virtual void Persist() => throw new NotImplementedException();

        private void BeforeAccessNotification(TokenCacheNotificationArgs args) => Load();
		

		// Triggered right after MSAL accessed the cache.
		void AfterAccessNotification(TokenCacheNotificationArgs args)
		{
			// if the access operation resulted in a cache update
			if (UsertokenCache.HasStateChanged)
			{
				Persist();
			}
		}
    }
}
