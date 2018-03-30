﻿using System;


[AttributeUsage(AttributeTargets.Class |
AttributeTargets.Constructor |
AttributeTargets.Field |
AttributeTargets.Method |
AttributeTargets.Property,
AllowMultiple = true)]
public class ReflectionAttribute : Attribute {

	public ReflectionAttribute()
    {

    }
}
