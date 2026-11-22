using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Люк на асфальте: красный прибавляет 5% скорости, зелёный отнимает 5%.
    /// Сам по себе люк — плоская картинка, столкновения с ним нет.
    /// </summary>
    public class Manhole2D : MonoBehaviour
    {
        public bool faster;
    }
}
