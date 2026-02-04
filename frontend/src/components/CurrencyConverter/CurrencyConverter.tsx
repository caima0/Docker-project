import React, { useState, useEffect } from 'react';
import { currencyService } from '../../services/api';

interface Rate {
    code: string;
    currency: string;
    bid: number;
    ask: number;
}

const CurrencyConverter: React.FC = () => {
    const [rates, setRates] = useState<Rate[]>([]);
    const [fromCurrency, setFromCurrency] = useState('');
    const [amount, setAmount] = useState('');
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchRates = async () => {
            try {
                const data = await currencyService.getRates();
                setRates(data);
                if (data.length > 0) {
                    setFromCurrency(data[0].code);
                }
            } catch (err) {
                setError('Failed to fetch rates');
            }
        };
        fetchRates();
    }, []);

    const handleConvert = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const response = await currencyService.convertCurrency(
                fromCurrency,
                'PLN',
                parseFloat(amount)
            );
            if (response.redirectUrl) {
                localStorage.setItem('pendingPayment', 'true');
                window.location.href = response.redirectUrl;
            }
        } catch (err) {
            setError('Conversion failed');
        }
    };

    return (
        <div className="converter">
            <h2>Currency Converter</h2>
            {error && <div className="error">{error}</div>}
            <form onSubmit={handleConvert}>
                <div>
                    <label>Amount:</label>
                    <input
                        type="number"
                        value={amount}
                        onChange={(e) => setAmount(e.target.value)}
                        required
                        min="0"
                        step="0.01"
                    />
                </div>
                <div>
                    <label>To Currency:</label>
                    <select
                        value={fromCurrency}
                        onChange={(e) => setFromCurrency(e.target.value)}
                        required
                    >
                        {rates.map((rate, index) => (
                            <option 
                                key={`${rate.code}-${rate.currency}-${index}`} 
                                value={rate.code}
                            >
                                {rate.code} - {rate.currency}
                            </option>
                        ))}
                    </select>
                </div>
                <div className="conversion-info">
                    <p>Converting from PLN (Polish Złoty)</p>
                </div>
                <button type="submit">Convert</button>
            </form>
        </div>
    );
};

export default CurrencyConverter; 