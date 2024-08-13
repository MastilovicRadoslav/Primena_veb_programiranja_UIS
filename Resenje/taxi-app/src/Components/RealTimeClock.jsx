import React, { useState, useEffect } from 'react';

const RealTimeClock = ({ arrivalMinutes, tripEndMinutes, onTripEnd }) => {
    const [arrivalTimeLeft, setArrivalTimeLeft] = useState(arrivalMinutes * 60);
    const [tripEndTimeLeft, setTripEndTimeLeft] = useState(tripEndMinutes * 60);
    const [rideStarted, setRideStarted] = useState(false);

    useEffect(() => {
        const arrivalTimer = setInterval(() => {
            setArrivalTimeLeft(prev => {
                if (prev > 0) {
                    return prev - 1;
                } else {
                    clearInterval(arrivalTimer);
                    setRideStarted(true);
                    return 0;
                }
            });
        }, 1000);

        return () => {
            clearInterval(arrivalTimer);
        };
    }, []);

    useEffect(() => {
        if (rideStarted) {
            const tripEndTimer = setInterval(() => {
                setTripEndTimeLeft(prev => (prev > 0 ? prev - 1 : 0));
            }, 1000);

            return () => {
                clearInterval(tripEndTimer);
            };
        }
    }, [rideStarted]);

    useEffect(() => {
        if (tripEndTimeLeft === 0) {
            onTripEnd(); // Call API or perform any action when trip ends
        }
    }, [tripEndTimeLeft, onTripEnd]);

    const formatTime = seconds => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins}:${secs < 10 ? '0' : ''}${secs}`;
    };

    return (
        <div style={{ color: 'white', marginTop: '30px' }}>
            {!rideStarted ? (
                <div>Time to arrive: {formatTime(arrivalTimeLeft)}</div>
            ) : (
                <div>
                    <div>Ride is started</div>
                    <div>Time to end ride: {formatTime(tripEndTimeLeft)}</div>
                </div>
            )}
        </div>
    );
};

export default RealTimeClock;
