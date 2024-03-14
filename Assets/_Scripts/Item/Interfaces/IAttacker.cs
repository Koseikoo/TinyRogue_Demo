using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using UniRx;

public interface IAttacker
{
    public List<Slot> ModSlots { get; set; }
}