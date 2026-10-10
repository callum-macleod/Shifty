
public enum Layers
{
    Default = 0,
    Player = 3,
    Wall = 6,
    Enemy = 7,
}

public class Utils
{
    public static int LayerToLayerMask(int layer)
    {
        return 1 << layer;
    }

    public static int LayerToLayerMask(Layers layer)
    {
        return 1 << (int)layer;
    }

    public static int LayersToLayerMask(Layers[] layers)
    {
        int layerMask = 0;
        foreach (Layers layer in layers)
            layerMask = layerMask | 1 << (int)layer;

        return layerMask;
    }
}
