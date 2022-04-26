//==============================================================================
/**
@file       %BC_NM%.cpp

@brief      %PL_NM% plugin

@copyright  %CP_RGT%

**/
//==============================================================================

#include "%BC_NM%.h"
#include <atomic>

#ifdef __APPLE__
	
#else
	
#endif

#include "Common/ESDConnectionManager.h"

class CallBackTimer
{
public:
    CallBackTimer() :_execute(false) { }

    ~CallBackTimer()
    {
        if( _execute.load(std::memory_order_acquire) )
        {
            stop();
        };
    }

    void stop()
    {
        _execute.store(false, std::memory_order_release);
        if(_thd.joinable())
            _thd.join();
    }

    void start(int interval, std::function<void(void)> func)
    {
        if(_execute.load(std::memory_order_acquire))
        {
            stop();
        };
        _execute.store(true, std::memory_order_release);
        _thd = std::thread([this, interval, func]()
        {
            while (_execute.load(std::memory_order_acquire))
            {
                func();
                std::this_thread::sleep_for(std::chrono::milliseconds(interval));
            }
        });
    }

    bool is_running() const noexcept
    {
        return (_execute.load(std::memory_order_acquire) && _thd.joinable());
    }

private:
    std::atomic<bool> _execute;
    std::thread _thd;
};

%BC_NM%::%BC_NM%()
{
	mInternalCounter = 0;
	mTimer = new CallBackTimer();
	mTimer->start(1000, [this]()
	{
		this->UpdateTimer();
	});
}

%BC_NM%::~%BC_NM%()
{
	if(mTimer != nullptr)
	{
		mTimer->stop();
		
		delete mTimer;
		mTimer = nullptr;
	}
}

void %BC_NM%::UpdateTimer()
{
	//
	// Warning: UpdateTimer() is running in the timer thread
	//
	if(mConnectionManager != nullptr)
	{
		SYSTEMTIME lLocalTime;

		GetLocalTime(&lLocalTime);

		//mInternalCounter++;
		mVisibleContextsMutex.lock();
		int currentValue = mInternalCounter%1000;// mCpuUsageHelper->GetCurrentCPUValue();
		for (const std::string& context : mVisibleContexts)
		{
			mConnectionManager->SetTitle(std::to_string(lLocalTime.wHour) /* + "%"*/, context, kESDSDKTarget_HardwareAndSoftware);
		}
		mVisibleContextsMutex.unlock();
	}

	
}

void %BC_NM%::KeyDownForAction(const std::string& inAction, const std::string& inContext, const json &inPayload, const std::string& inDeviceID)
{
	// Nothing to do
	//mConnectionManager->SetImage("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", inContext, kESDSDKTarget_HardwareAndSoftware);
	
}

void %BC_NM%::KeyUpForAction(const std::string& inAction, const std::string& inContext, const json &inPayload, const std::string& inDeviceID)
{
	// Nothing to do
}

void %BC_NM%::WillAppearForAction(const std::string& inAction, const std::string& inContext, const json &inPayload, const std::string& inDeviceID)
{
	// Remember the context
	mVisibleContextsMutex.lock();
	mVisibleContexts.insert(inContext);
	mVisibleContextsMutex.unlock();
}

void %BC_NM%::WillDisappearForAction(const std::string& inAction, const std::string& inContext, const json &inPayload, const std::string& inDeviceID)
{
	// Remove the context
	mVisibleContextsMutex.lock();
	mVisibleContexts.erase(inContext);
	mVisibleContextsMutex.unlock();
}

void %BC_NM%::DeviceDidConnect(const std::string& inDeviceID, const json &inDeviceInfo)
{
	// Nothing to do
}

void %BC_NM%::DeviceDidDisconnect(const std::string& inDeviceID)
{
	// Nothing to do
}

void %BC_NM%::SendToPlugin(const std::string& inAction, const std::string& inContext, const json &inPayload, const std::string& inDeviceID)
{
	// Nothing to do
}
