﻿namespace beholder_nest.Routing
{
  using System;

  public interface ITypeActivatorCache
  {
    TInstance CreateInstance<TInstance>(IServiceProvider serviceProvider, Type implementationType);
  }
}