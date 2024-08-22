import React, { useState, useEffect } from 'react';
import '../Styles/DriversViewPage.css'; // Import your CSS file for styling
import { getAllRidesAdmin } from '../Services/AdminServices';

export default function RidesAdmin() {
    const [rides, setRides] = useState([]);
    const token = localStorage.getItem('token');
    const apiEndpoint = process.env.REACT_APP_GET_ALL_RIDES_ADMIN; // 
    // Funckija za dobavljanje svih voznji
    const fetchDrivers = async () => {
        try {
            const data = await getAllRidesAdmin(token, apiEndpoint);
            console.log("Rides:", data.rides);
            setRides(data.rides);
        } catch (error) {
            console.error('Error fetching drivers:', error);
        }
    };

    useEffect(() => {
        fetchDrivers(); // Pozivanje funkcije za dobavljanje vožnji nakon renderovanja komponente
    }, []);

    

    return (
        <div className="centered" style={{ width: '100%', height: '20%' }}>
            <table className="styled-table">
                <thead>
                    <tr>
                        <th>From</th>
                        <th>To</th>
                        <th>Price</th>
                        <th>Accepted by driver</th>
                        <th>Finished</th>
                    </tr>
                </thead>
                <tbody>
                    {rides.map((ride, index) => (
                        <tr key={index}>
                            <td>{ride.currentLocation}</td>
                            <td>{ride.destination}</td>
                            <td>{ride.price}</td>
                            <td>{ride.accepted ? "Yes" : "No"}</td>
                            <td>{ride.isFinished ? "Yes" : "No"}</td>
                        </tr>
                    ))}
                </tbody>
            </table>

        </div>
    );
}
