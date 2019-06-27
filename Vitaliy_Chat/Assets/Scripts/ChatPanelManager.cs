﻿using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatInfo
{
    public string chatKey { set; get; }
    public string chatUser { set; get; }
    public string chatText { set; get; }
    public string chatEdited { set; get; }
    public string chatEditing { set; get; }
}

public class ChatPanelManager : MonoBehaviour {

    public GameObject chatItemObj;
    public GameObject chatListObj;

    public UIInput mInput;

    GameObject loginPanel;
    GameObject roomPanel;
    GameObject chatPanel;
    
    List<ChatInfo> m_ChatList = new List<ChatInfo>();

    string editChatKey = null;
    string editChatUser = null;
    string editChatTextBefore = null;

	// Use this for initialization
	void Start ()
    {
        mInput.label.maxLineCount = 1;

        loginPanel = transform.parent.Find("LoginPanel").gameObject;
        roomPanel = transform.parent.Find("RoomPanel").gameObject;
        chatPanel = transform.parent.Find("ChatPanel").gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        if(m_ChatList.Count > 0)
        {
            foreach (ChatInfo eachInfo in m_ChatList)
                AddChatToChatView(eachInfo.chatKey, eachInfo.chatUser, eachInfo.chatText, eachInfo.chatEdited, eachInfo.chatEditing);
            chatListObj.GetComponent<UIGrid>().Reposition();
            chatListObj.transform.parent.GetComponent<UIScrollView>().ResetPosition();
            chatListObj.SetActive(false);
            chatListObj.SetActive(true);
            ClearChatList();
        }
    }

    private void OnDisable()
    {
        int childCnt = chatListObj.transform.childCount;
        for (int i = childCnt - 1; i >= 0; i--)
            Destroy(chatListObj.transform.GetChild(i).gameObject);
    }

    private void OnEnable()
    {
        editChatTextBefore = null;
        editChatKey = null;
        editChatUser = null;

        ClearChatList();
        string currentRoomNo = AppManager.Instance.currentRoomNo;

        mInput.value = "";
        mInput.gameObject.GetComponent<BoxCollider>().enabled = AppManager.Instance.bUserType;

        FirebaseDatabase.DefaultInstance.GetReference("Messages").Child(currentRoomNo).ChildAdded += HandleMessageAdded;
        FirebaseDatabase.DefaultInstance.GetReference("Messages").Child(currentRoomNo).ChildChanged += HandleMessageChanged;
    }

    void HandleMessageAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        int nDbIterator = 0;
        //Debug.Log("HandleMessageAdded " + args.Snapshot);
        ChatInfo newChatInfo = new ChatInfo();
        newChatInfo.chatKey = args.Snapshot.Key;
        newChatInfo.chatEdited = null;
        newChatInfo.chatEditing = null;
        foreach (DataSnapshot aaaChat in args.Snapshot.Children)
        {
            //Debug.Log(aaaChat);
            if(nDbIterator == 0)
            {
                newChatInfo.chatUser = aaaChat.Key;
                newChatInfo.chatText = (string)aaaChat.Value;
            }
            else if(aaaChat.Key.Equals("Edited"))
                newChatInfo.chatEdited = (string)aaaChat.Value;
            else if (aaaChat.Key.Equals("Editing"))
                newChatInfo.chatEditing = (string)aaaChat.Value;

            nDbIterator++;
        }
        m_ChatList.Add(newChatInfo);
    }

    void HandleMessageChanged(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        int nDbIterator = 0;
        //Debug.Log("HandleMessageChanged " + args.Snapshot);
        ChatInfo newChatInfo = new ChatInfo();
        newChatInfo.chatKey = args.Snapshot.Key;
        newChatInfo.chatEdited = null;
        newChatInfo.chatEditing = null;
        foreach (DataSnapshot bbbChat in args.Snapshot.Children)
        {
            //Debug.Log(bbbChat);
            if (nDbIterator == 0)
            {
                newChatInfo.chatUser = bbbChat.Key;
                newChatInfo.chatText = (string)bbbChat.Value;
            }
            else if (bbbChat.Key.Equals("Edited"))
                newChatInfo.chatEdited = (string)bbbChat.Value;
            else if (bbbChat.Key.Equals("Editing"))
                newChatInfo.chatEditing = (string)bbbChat.Value;

            nDbIterator++;
        }
        m_ChatList.Add(newChatInfo);
    }

    void ClearChatList()
    {
        m_ChatList.Clear();
    }

    void AddChatToChatView(string chatkey, string username, string text, string editedStr, string editingStr)
    {
        bool bOldItem = false;
        for(int i=0; i<chatListObj.transform.childCount; i++)
        {
            if(chatListObj.transform.GetChild(i).gameObject.name.Equals(chatkey))
            {
                bOldItem = true;
                break;
            }
        }
        
        if (bOldItem)
        {
            GameObject tempItemObj = chatListObj.transform.Find(chatkey).gameObject;
            InitValueWithChatList(tempItemObj, chatkey, username, text, editedStr, editingStr);
        }
        else
        {
            if (chatListObj.transform.childCount >= 10)
                DestroyImmediate(chatListObj.transform.GetChild(0).gameObject);

            GameObject tempItemObj = NGUITools.AddChild(chatListObj, chatItemObj);
            tempItemObj.transform.localScale = Vector3.one;
            tempItemObj.SetActive(true);
            InitValueWithChatList(tempItemObj, chatkey, username, text, editedStr, editingStr);
        }
    }

    void InitValueWithChatList(GameObject tempItemObj, string chatkey, string username, string text, string editedStr, string editingStr)
    {
        if (AppManager.Instance.user.DisplayName.Equals(username))
            username = "YOU:";
        else
            username = username + ":";
        tempItemObj.transform.Find("Name").gameObject.GetComponent<UILabel>().text = username;
        tempItemObj.transform.Find("Text").gameObject.GetComponent<UILabel>().text = text;
        if (editingStr != null)
        {
            tempItemObj.transform.Find("Edit").gameObject.SetActive(true);
            tempItemObj.transform.Find("Edit").gameObject.GetComponent<UILabel>().text = editingStr + " is editing now...";
        }
        else if (editedStr != null)
        {
            tempItemObj.transform.Find("Edit").gameObject.SetActive(true);
            if (editedStr.Equals(AppManager.Instance.user.DisplayName))
                editedStr = "YOU";
            tempItemObj.transform.Find("Edit").gameObject.GetComponent<UILabel>().text = editedStr + " edited";
        }
        else
            tempItemObj.transform.Find("Edit").gameObject.SetActive(false);
        tempItemObj.name = chatkey;
        tempItemObj.transform.Find("Pencil").gameObject.SetActive(AppManager.Instance.bUserType);
    }

    public void OnBackBtnClicked()
    {
        if (editChatKey != null)
            return;
        loginPanel.SetActive(false);
        roomPanel.SetActive(true);
        chatPanel.SetActive(false);
    }

    public void OnSubmit()
    {
        string currentRoomNo = AppManager.Instance.currentRoomNo;
        string userId = AppManager.Instance.user.UserId;
        string userName = AppManager.Instance.user.DisplayName;
        // It's a good idea to strip out all symbols as we don't want user input to alter colors, add new lines, etc
        string text = NGUIText.StripSymbols(mInput.value);

        if (!string.IsNullOrEmpty(text))
        {
            mInput.value = "";
            // mInput.isSelected = false;

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

            if(editChatKey != null)
            {
                reference.Child("Messages").Child(currentRoomNo).Child(editChatKey).Child("Editing").RemoveValueAsync();
                if(!editChatTextBefore.Equals(text))
                {
                    reference.Child("Messages").Child(currentRoomNo).Child(editChatKey).Child(editChatUser).SetValueAsync(text);
                    reference.Child("Messages").Child(currentRoomNo).Child(editChatKey).Child("Edited").SetValueAsync(AppManager.Instance.user.DisplayName);
                }

                editChatTextBefore = null;
                editChatKey = null;
                editChatUser = null;
            }
            else
                reference.Child("Messages").Child(currentRoomNo).Push().Child(userName).SetValueAsync(text);

        }
    }

    public void OnPencilClicked(GameObject obj)
    {
        if (editChatKey != null)
        {
            Debug.Log("You are editing other one now...");
            return;
        }
        string editStr = obj.transform.Find("Edit").gameObject.GetComponent<UILabel>().text;
        if(editStr.Contains("is editing now..."))
        {
            Debug.Log("Someone is editing this now...");
            return;
        }

        editChatKey = obj.name;
        Debug.Log("OnPencilClicked " + editChatKey);
        editChatUser = obj.transform.Find("Name").gameObject.GetComponent<UILabel>().text;
        editChatUser = editChatUser.Substring(0, editChatUser.Length - 1);
        if (editChatUser.Equals("YOU"))
            editChatUser = AppManager.Instance.user.DisplayName;
        string currentRoomNo = AppManager.Instance.currentRoomNo;
        string userName = AppManager.Instance.user.DisplayName;

        mInput.value = obj.transform.Find("Text").gameObject.GetComponent<UILabel>().text;
        editChatTextBefore = obj.transform.Find("Text").gameObject.GetComponent<UILabel>().text;

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("Messages").Child(currentRoomNo).Child(editChatKey).Child("Editing").SetValueAsync(userName);
    }

    private void OnApplicationQuit()
    {
        string currentRoomNo = AppManager.Instance.currentRoomNo;
        if (editChatKey != null)
        {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("Messages").Child(currentRoomNo).Child(editChatKey).Child("Editing").RemoveValueAsync();
        }
    }
}