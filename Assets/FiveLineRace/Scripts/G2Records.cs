using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveLineRace
{
    /// <summary>Один заезд в таблице рекордов.</summary>
    [Serializable]
    public class G2Record
    {
        public int score;
        public int distance;
        public int coins;

        public G2Record() { }

        public G2Record(int score, int distance, int coins)
        {
            this.score = score;
            this.distance = distance;
            this.coins = coins;
        }
    }

    [Serializable]
    class G2RecordBook
    {
        public List<G2Record> items = new List<G2Record>();
    }

    /// <summary>
    /// Таблица рекордов: до десяти лучших заездов в PlayerPrefs.
    /// Сортировка по счёту, при равенстве — по дистанции.
    /// </summary>
    public static class G2Records
    {
        const string Key = "FiveLineRace.Records";

        public const int Max = 10;

        public static List<G2Record> Load()
        {
            var list = new List<G2Record>();
            string json = PlayerPrefs.GetString(Key, string.Empty);

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var book = JsonUtility.FromJson<G2RecordBook>(json);
                    if (book != null && book.items != null) list = book.items;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[FiveLineRace] Не читается таблица рекордов: " + e.Message);
                }
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null) list.RemoveAt(i);
            }

            list.Sort(Compare);

            if (list.Count > Max) list.RemoveRange(Max, list.Count - Max);
            return list;
        }

        /// <summary>
        /// Добавляет заезд в таблицу. Возвращает место (1..Max) или 0,
        /// если результат не попал в таблицу.
        /// </summary>
        public static int Submit(int score, int distance, int coins)
        {
            var list = Load();
            var entry = new G2Record(score, Mathf.Max(0, distance), Mathf.Max(0, coins));

            int place = list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                if (score > list[i].score)
                {
                    place = i;
                    break;
                }
            }

            list.Insert(place, entry);
            if (list.Count > Max) list.RemoveRange(Max, list.Count - Max);
            Save(list);

            return place < Max ? place + 1 : 0;
        }

        static int Compare(G2Record a, G2Record b)
        {
            int byScore = b.score.CompareTo(a.score);
            if (byScore != 0) return byScore;

            int byDistance = b.distance.CompareTo(a.distance);
            if (byDistance != 0) return byDistance;

            return b.coins.CompareTo(a.coins);
        }

        static void Save(List<G2Record> list)
        {
            var book = new G2RecordBook();
            book.items = list;

            PlayerPrefs.SetString(Key, JsonUtility.ToJson(book));
            PlayerPrefs.Save();
        }
    }
}
