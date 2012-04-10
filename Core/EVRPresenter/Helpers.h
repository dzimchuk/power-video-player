//////////////////////////////////////////////////////////////////////////
//
// Helpers.cpp : Miscellaneous helpers.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//
//////////////////////////////////////////////////////////////////////////

#pragma once

//-----------------------------------------------------------------------------
// SamplePool class
//
// Manages a list of allocated samples.
//-----------------------------------------------------------------------------

class SamplePool
{
public:
    SamplePool();
    virtual ~SamplePool();

    HRESULT Initialize(VideoSampleList& samples);
    HRESULT Clear();

    HRESULT GetSample(IMFSample **ppSample);    // Does not block.
    HRESULT ReturnSample(IMFSample *pSample);
    BOOL    AreSamplesPending();

private:
    CRITICAL_SECTION            m_lock;

    VideoSampleList             m_VideoSampleQueue;         // Available queue

    BOOL                        m_bInitialized;
    DWORD                       m_cPending;
};


//-----------------------------------------------------------------------------
// ThreadSafeQueue template
// Thread-safe queue of COM interface pointers.
//
// T: COM interface type.
//
// This class is used by the scheduler.
//
// Note: This class uses a critical section to protect the state of the queue.
// With a little work, the scheduler could probably use a lock-free queue.
//-----------------------------------------------------------------------------

template <class T>
class ThreadSafeQueue
{
public:

    ThreadSafeQueue()
    {
        InitializeCriticalSection(&m_lock);
    }
    virtual ~ThreadSafeQueue()
    {
        DeleteCriticalSection(&m_lock);
    }


    HRESULT Queue(T *p)
    {
        EnterCriticalSection(&m_lock);

        HRESULT hr = m_list.InsertBack(p);

        LeaveCriticalSection(&m_lock);
        return hr;
    }

    HRESULT Dequeue(T **pp)
    {
        EnterCriticalSection(&m_lock);

        HRESULT hr = S_OK;

        if (m_list.IsEmpty())
        {
            *pp = NULL;
            hr = S_FALSE;
        }
        else
        {
            hr = m_list.RemoveFront(pp);
        }

        LeaveCriticalSection(&m_lock);
        return hr;
    }

    HRESULT PutBack(T *p)
    {
        EnterCriticalSection(&m_lock);

        HRESULT hr =  m_list.InsertFront(p);

        LeaveCriticalSection(&m_lock);
        return hr;
    }

    void Clear()
    {
        EnterCriticalSection(&m_lock);

        m_list.Clear();

        LeaveCriticalSection(&m_lock);
    }


private:
    CRITICAL_SECTION    m_lock;
    ComPtrList<T>       m_list;
};

