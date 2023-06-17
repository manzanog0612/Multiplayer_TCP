using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

using Game.RoomSelection.RoomsView;

namespace Game.RoomSelection
{
    public class RoomSelectionView : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [Header("Buttons")]
        [SerializeField] private Button btnBack = null;
        [SerializeField] private Button btnEnterRoom = null;
        [SerializeField] private Button btnCreateRoom = null;

        [Header("RoomView Configurations")]
        [SerializeField] private GameObject roomViewPrefab = null;
        [SerializeField] private Transform roomViewsHolder = null;
        #endregion

        #region PRIVATE_FIELDS
        private ObjectPool<RoomView> roomViewsPool = null;
        private List<RoomView> roomViews = new List<RoomView>();

        private RoomData selectedRoomData = null;
        #endregion

        #region PROPERTIES
        public RoomData SelectedRoomData { get => selectedRoomData; }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action onPressBack, Action onPressEnterRoom, Action onPressCreateRoom)
        {
            btnBack.onClick.AddListener(() => onPressBack.Invoke());
            btnEnterRoom.onClick.AddListener(() => onPressEnterRoom.Invoke());
            btnCreateRoom.onClick.AddListener(() => onPressCreateRoom.Invoke());

            roomViewsPool = new ObjectPool<RoomView>(CreateRoomView, OnGetRoomView, OnReleaseRoomView);
        }

        public void CreateRoomViews(RoomData[] roomDatas)
        {
            for (int i = 0; i < roomViews.Count; i++)
            {
                roomViewsPool.Release(roomViews[i]);
            }

            roomViewsPool.Clear();

            for (int i = 0; i < roomDatas.Length; i++)
            {
                RoomView roomView = roomViewsPool.Get();
                roomView.Configure(roomDatas[i], OnSelectRoomView);
                roomViews.Add(roomView);
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnSelectRoomView(RoomData roomData, RoomView roomView)
        {
            selectedRoomData = roomData;

            for (int i = 0; i < roomViews.Count; i++)
            {
                if (roomViews[i] != roomView)
                {
                    roomViews[i].ToggleSelected(false);
                }
            }
        }
        #endregion

        #region POOL_METHODS
        private RoomView CreateRoomView()
        {
            return Instantiate(roomViewPrefab, roomViewsHolder).GetComponent<RoomView>();
        }

        private void OnGetRoomView(RoomView roomView)
        {
            roomView.gameObject.SetActive(true);
        }

        private void OnReleaseRoomView(RoomView roomView)
        {
            roomView.gameObject.SetActive(false);
        }
        #endregion
    }
}
