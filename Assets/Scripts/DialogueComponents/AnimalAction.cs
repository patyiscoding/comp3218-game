using System;

[Serializable]
public class AnimalAction
{
    public string condition;
    public string animalName;
    public string action;
    public bool fromAnimator = true;
    public bool pluralAnimals = false;
    public bool endNodeAnimation = false;
    public bool includeChildrenLayers = false;
}