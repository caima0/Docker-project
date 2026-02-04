import React, { useState, useEffect } from 'react';
import api from '../../services/api';
import './Balance.css';

interface BalanceItem {
    currency: string;
    amount: number;
    lastUpdated: string;
}

const Balance: React.FC = () => {
    const [balances, setBalances] = useState<BalanceItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string>('');

    const fetchBalances = async () => {
        try {
            const response = await api.get('/Payment/balance');
            setBalances(response.data);
            setError('');
        } catch (err: any) {
            setError('Failed to fetch balances');
            console.error('Error fetching balances:', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchBalances();
    }, []);

    if (loading) return <div className="balance-loading">Loading balances...</div>;
    if (error) return <div className="balance-error">{error}</div>;

    return (
        <div className="balance-container">
            <h2>Your Balances</h2>
            <div className="balance-grid">
                {balances.map((balance) => (
                    <div key={balance.currency} className="balance-card">
                        <div className="balance-currency">{balance.currency}</div>
                        <div className="balance-amount">{balance.amount.toFixed(2)}</div>
                        <div className="balance-date">
                            Last updated: {new Date(balance.lastUpdated).toLocaleString()}
                        </div>
                    </div>
                ))}
                {balances.length === 0 && (
                    <div className="no-balances">
                        No balances found. Make a payment to add funds to your account.
                    </div>
                )}
            </div>
        </div>
    );
};

export default Balance; 