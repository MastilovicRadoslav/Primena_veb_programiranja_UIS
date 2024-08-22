import React, { useState, useEffect } from 'react';
import '../Styles/DriversViewPage.css'; // Import your CSS file for styling
import { getMyRidesDriver } from '../Services/DriverServices';

export default function RidesDrivers() { //Isto sve kao i kod RidesRider samo za driverID to jest voznje za odredjenog vozaca
    const [rides, setRides] = useState([]);
    const token = localStorage.getItem('token');
    const apiEndpoint = process.env.REACT_APP_GET_ALL_RIDES_DRIVER;
    // Funkcija za dobavljanje svih voznji za vozaca
    const fetchDrivers = async () => {
        try {
            const data = await getMyRidesDriver(token, apiEndpoint, localStorage.getItem('userId'));
            console.log("Rides:", data.rides);
            setRides(data.rides);
        } catch (error) {
            console.error('Error fetching drivers:', error);
        }
    };

    useEffect(() => {
        fetchDrivers();
    }, []);



    return (
        <div className="centered" style={{ width: '100%', height: '10%' }}>
            {rides && rides.length > 0 ? (
                <table className="styled-table">
                    <thead>
                        <tr>
                            <th>From</th>
                            <th>To</th>
                            <th>Price</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rides.map((ride, index) => (
                            <tr key={index}>
                                <td>{ride.currentLocation}</td>
                                <td>{ride.destination}</td>
                                <td>{ride.price}&euro;</td>

                            </tr>
                        ))}
                    </tbody>
                </table>
            ) : (
                <p>No rides available</p>
            )}
        </div>
    );
}
