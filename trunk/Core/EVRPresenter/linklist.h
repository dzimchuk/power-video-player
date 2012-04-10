//-----------------------------------------------------------------------------
// File: Linklist.h
// Desc: Linked list class.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
//  Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#pragma once

#include <new>
#include <assert.h>

/*---------------------------------------------------------------------

 ComPtrList template

 Implements a list of COM pointers.

 Template parameters:

    T:        COM interface type.

    NULLABLE: If true, client can insert NULL pointers. By default,
              the list does not allow NULL pointers.

 Examples:

     ComPtrList<IUnknown>
     ComPtrList<IUnknown, TRUE>


 The list supports enumeration:

     for (ComPtrList<IUnknown>::POSIITON pos = list.FrontPosition();
          pos != list.EndPosition();
          pos = list.Next(pos))
     {
         IUnknown *pUnk = NULL;

         hr = list.GetItemByPosition(pos, &pUnk);

         if (SUCCEEDED(hr))
         {
             pUnk->Release();
         }
     }

 The list is implemented as a double-linked list with an anchor node.

---------------------------------------------------------------------*/

template <class T, bool NULLABLE = FALSE>
class ComPtrList
{
protected:

    typedef T* Ptr;

    // Nodes in the linked list
    struct Node
    {
        Node *prev;
        Node *next;
        Ptr  item;

        Node() : prev(NULL), next(NULL), item(NULL)
        {
        }

        Node(Ptr item) : prev(NULL), next(NULL)
        {
            this->item = item;
            if (item)
            {
                item->AddRef();
            }
        }
        ~Node()
        {
            if (item)
            {
                item->Release();
                item = NULL;
            }
        }

        Ptr Item() const { return item; }
    };

public:

    // Object for enumerating the list.
    class POSITION
    {
        friend class ComPtrList<T>;

    public:
        POSITION() : pNode(NULL)
        {
        }

        bool operator==(const POSITION &p) const
        {
            return pNode == p.pNode;
        }

        bool operator!=(const POSITION &p) const
        {
            return pNode != p.pNode;
        }

    private:
        const Node *pNode;

        POSITION(Node *p) : pNode(p)
        {
        }
    };

protected:
    Node    m_anchor;  // Anchor node for the linked list.
    DWORD   m_count;   // Number of items in the list.

    Node* Front() const
    {
        return m_anchor.next;
    }

    Node* Back() const
    {
        return m_anchor.prev;
    }

    virtual HRESULT InsertAfter(Ptr item, Node *pBefore)
    {
        if (pBefore == NULL)
        {
            return E_POINTER;
        }

        // Do not allow NULL item pointers unless NULLABLE is true.
        if (!item && !NULLABLE)
        {
            return E_POINTER;
        }

        Node *pNode = new (std::nothrow) Node(item);
        if (pNode == NULL)
        {
            return E_OUTOFMEMORY;
        }

        Node *pAfter = pBefore->next;

        pBefore->next = pNode;
        pAfter->prev = pNode;

        pNode->prev = pBefore;
        pNode->next = pAfter;

        m_count++;

        return S_OK;
    }

    virtual HRESULT GetItem(const Node *pNode, Ptr* ppItem)
    {
        if (pNode == NULL || ppItem == NULL)
        {
            return E_POINTER;
        }

        *ppItem = pNode->item;

        if (*ppItem)
        {
            (*ppItem)->AddRef();
        }

        return S_OK;
    }

    // RemoveItem:
    // Removes a node and optionally returns the item.
    // ppItem can be NULL.
    virtual HRESULT RemoveItem(Node *pNode, Ptr *ppItem)
    {
        if (pNode == NULL)
        {
            return E_POINTER;
        }

        assert(pNode != &m_anchor); // We should never try to remove the anchor node.
        if (pNode == &m_anchor)
        {
            return E_INVALIDARG;
        }


        Ptr item;

        // The next node's previous is this node's previous.
        pNode->next->prev = pNode->prev;

        // The previous node's next is this node's next.
        pNode->prev->next = pNode->next;

        item = pNode->item;

        if (ppItem)
        {
            *ppItem = item;

            if (*ppItem)
            {
                (*ppItem)->AddRef();
            }
        }

        delete pNode;
        m_count--;

        return S_OK;
    }

public:

    ComPtrList()
    {
        m_anchor.next = &m_anchor;
        m_anchor.prev = &m_anchor;

        m_count = 0;
    }

    virtual ~ComPtrList()
    {
        Clear();
    }

    void Clear()
    {
        Node *n = m_anchor.next;

        // Delete the nodes
        while (n != &m_anchor)
        {
            Node *tmp = n->next;
            delete n;
            n = tmp;
        }

        // Reset the anchor to point at itself
        m_anchor.next = &m_anchor;
        m_anchor.prev = &m_anchor;

        m_count = 0;
    }

    // Insertion functions
    HRESULT InsertBack(Ptr item)
    {
        return InsertAfter(item, m_anchor.prev);
    }


    HRESULT InsertFront(Ptr item)
    {
        return InsertAfter(item, &m_anchor);
    }

    // RemoveBack: Removes the tail of the list and returns the value.
    // ppItem can be NULL if you don't want the item back.
    HRESULT RemoveBack(Ptr *ppItem)
    {
        if (IsEmpty())
        {
            return E_FAIL;
        }
        else
        {
            return RemoveItem(Back(), ppItem);
        }
    }

    // RemoveFront: Removes the head of the list and returns the value.
    // ppItem can be NULL if you don't want the item back.
    HRESULT RemoveFront(Ptr *ppItem)
    {
        if (IsEmpty())
        {
            return E_FAIL;
        }
        else
        {
            return RemoveItem(Front(), ppItem);
        }
    }

    // GetBack: Gets the tail item.
    HRESULT GetBack(Ptr *ppItem)
    {
        if (IsEmpty())
        {
            return E_FAIL;
        }
        else
        {
            return GetItem(Back(), ppItem);
        }
    }

    // GetFront: Gets the front item.
    HRESULT GetFront(Ptr *ppItem)
    {
        if (IsEmpty())
        {
            return E_FAIL;
        }
        else
        {
            return GetItem(Front(), ppItem);
        }
    }


    // GetCount: Returns the number of items in the list.
    DWORD GetCount() const { return m_count; }

    bool IsEmpty() const
    {
        return (GetCount() == 0);
    }

    // Enumerator functions

    POSITION FrontPosition()
    {
        if (IsEmpty())
        {
            return POSITION(NULL);
        }
        else
        {
            return POSITION(Front());
        }
    }

    POSITION EndPosition() const
    {
        return POSITION();
    }

    HRESULT GetItemByPosition(POSITION pos, Ptr *ppItem)
    {
        if (pos.pNode)
        {
            return GetItem(pos.pNode, ppItem);
        }
        else
        {
            return E_FAIL;
        }
    }

    POSITION Next(const POSITION pos)
    {
        if (pos.pNode && (pos.pNode->next != &m_anchor))
        {
            return POSITION(pos.pNode->next);
        }
        else
        {
            return POSITION(NULL);
        }
    }
};

