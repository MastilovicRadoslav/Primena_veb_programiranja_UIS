import React, { useState, useEffect } from 'react';
import '../Styles/DriversViewPage.css';
import { GetDriversForVerify, VerifyDriver } from '../Services/AdminServices.js';
import { FaCheckCircle } from 'react-icons/fa';
import '../Styles/VerificationPage.css';

export default function VerifyDrivers() {
    const [drivers, setDrivers] = useState([]); //cuvanje liste vozaca koji cekaju na verifikaciju
    const token = localStorage.getItem('token'); //token
    const getAllDriversEndpoint = process.env.REACT_APP_GET_GET_NOT_VERIFIED_AND_VERIFIED_DRIVER;
    const verifyDriversEndpoint = process.env.REACT_APP_VERIFY_DRIVER;

    // Dohvatanje svih vozaca
    const fetchDrivers = async () => {
        try {
            const data = await GetDriversForVerify(getAllDriversEndpoint, token); //preuzimanje liste vozaca koji cekaju na verifikaciju
            console.log("Drivers for verify:", data); //ispis
            setDrivers(data.drivers); //setujem niz vozaca
        } catch (error) {
            console.error('Error fetching drivers:', error);
        }
    };

    const handleAccept = async (val) => {
        const { id, email } = val;
        try {
            const data = await VerifyDriver(verifyDriversEndpoint, id, "Prihvacen", email, token); //oznacujem vozaca kao "Prihvacen" verifikovan
            console.log("Drivers for verify:", data);//ispis
            //setDrivers(data.drivers);
        } catch (error) {
            console.error('Error fetching drivers:', error);
        }
    };
    const handleDecline = async (val) => {
        const { id, email } = val;
        console.log(id);
        console.log(email);
        try {
            const data = await VerifyDriver(verifyDriversEndpoint, id, "Odbijen", email, token); //odbijanje korisnika "Odbijen"
            console.log("Drivers for verify:", data);
        } catch (error) {
            console.error('Error fetching drivers:', error);
        }
    };

    useEffect(() => { //svaki put kad se niz [drivers] promeni useEffect se pokrene, automatski osvezujem listu vozaca nakon sto se neki vozac prihvati ili odbije
        fetchDrivers();
    }, [drivers]);

    return (
        <div className="centered">
            <table className="styled-table">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Last Name</th>
                        <th>Email</th>
                        <th>Username</th>
                        <th>Verification status</th>
                    </tr>
                </thead>
                <tbody>
                    {drivers.map((val) => (
                        <tr key={val.id}>
                            <td>{val.name}</td>
                            <td>{val.lastName}</td>
                            <td>{val.email}</td>
                            <td>{val.username}</td>
                            <td className="icon-container">
                                {val.status === 1 ? (
                                    <FaCheckCircle color="green" />
                                ) : (
                                    <>
                                        <button
                                            className="custom-button accept-button"
                                            onClick={() => handleAccept(val)} 
                                        >
                                            Accept
                                        </button>
                                        <button
                                            className="custom-button decline-button"
                                            onClick={() => handleDecline(val)}
                                        >
                                            Decline
                                        </button>
                                    </>
                                )}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
