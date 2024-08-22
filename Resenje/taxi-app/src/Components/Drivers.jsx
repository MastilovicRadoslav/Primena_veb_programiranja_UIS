import React, { useState, useEffect } from 'react';
import '../Styles/DriversViewPage.css'; // Import your CSS file for styling
import { ChangeDriverStatus, GetAllDrivers } from '../Services/AdminServices.js'; //bekend

export default function DriversView() {
    const [drivers, setDrivers] = useState([]); //niz sa podacima o vozacima, preuzet cu podatke sa servera
    const token = localStorage.getItem('token'); //token koji sam sacuvao u localStorage
    const apiEndpoint = process.env.REACT_APP_CHANGE_DRIVER_STATUS; //za promjenu statusa vozaca 
    const getAllDriversEndpoint = process.env.REACT_APP_GET_ALL_DRIVERS; //preuzimanje svih vozaca

    // Funkcija za preuzimanje vozaca
    const fetchDrivers = async () => {
        try {
            const data = await GetAllDrivers(getAllDriversEndpoint, token); //preuizmnaje svih podataka o vozacima
            console.log("Drivers:",data);
            setDrivers(data.drivers); //popunimo niz
        } catch (error) {
            console.error('Error fetching drivers:', error);
        }
    };

    useEffect(() => { //cim se komponenta renderuje pokrece se funkcija za preuzimanje vozaca
        fetchDrivers();
    }, []);

    const handleChangeStatus = async (id, isBlocked) => {
        try {
            await ChangeDriverStatus(apiEndpoint, id, !isBlocked, token); // funkcija koja menja status vozača
            await fetchDrivers(); // Kad se promeni status ponovo se preuzima lista vozaca 
        } catch (error) {
            console.error('Error changing driver status:', error);
        }
    };

    return (
        <div className="centered" style={{ width: '100%', height: '10%' }}>
            <table className="styled-table">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Last Name</th>
                        <th>Email</th>
                        <th>Username</th>
                        <th>Average rating</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    {drivers.map((val, key) => (
                        <tr key={val.id}>
                            <td>{val.name}</td>
                            <td>{val.lastName}</td>
                            <td>{val.email}</td>
                            <td>{val.username}</td>
                            <td>{val.averageRating}</td>
                            <td>
                                {val.isBlocked ? (
                                    <button className="green-button" onClick={() => handleChangeStatus(val.id, val.isBlocked)}>Unblock</button>
                                ) : (
                                    <button className="red-button" onClick={() => handleChangeStatus(val.id, val.isBlocked)}>Block</button>
                                )}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
