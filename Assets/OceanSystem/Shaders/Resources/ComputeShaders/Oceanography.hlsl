﻿#if !defined(OCEANOGRAPHY_INCLUDED)

#define OCEANOGRAPHY_INCLUDED

#define OCEANOGRAPHY_PI 3.1415926
static const float g = 9.81;
static const float sigmaOverRho = 0.074e-3;

float Depth;

struct SpectrumParams
{
	int energySpectrum;
	float windSpeed;
	float fetch;
	float peaking;
	float scale;
	float shortWavesFade;
	float alignment;
	float extraAlignment;
};

//---------------------------------
// Dispersion relations
//---------------------------------

float Frequency(float k, float depth)
{
	//return sqrt(g * k * tanh(min(k * depth, 20)));
	return sqrt((g * k + sigmaOverRho * k * k * k) * tanh(min(k * depth, 10)));
}

float FrequencyDerivative(float k, float depth)
{
	float th = tanh(min(k * depth, 10));
	float freq = Frequency(k, depth);
	float ch = cosh(min(k * depth, 10));
	
	//return g * (depth * k / ch / ch + th) / freq / 2;
	return (depth * (sigmaOverRho * k * k * k + g * k) / ch / ch
		+ (g + 3 * sigmaOverRho * k * k) * th) * 0.5 / freq;
}

//---------------------------------
// Directional spreads
//---------------------------------

float NormalisationFactor(float s)
{
	float s2 = s * s;
	float s3 = s2 * s;
	float s4 = s3 * s;
	if (s < 5)
		return -0.000564 * s4 + 0.00776 * s3 - 0.044 * s2 + 0.192 * s + 0.163;
	else
		return -4.80e-08 * s4 + 1.07e-05 * s3 - 9.53e-04 * s2 + 5.90e-02 * s + 3.93e-01;
}

float SpreadPowerHasselman(float omega, float peakOmega, float u)
{
	if (omega > peakOmega)
	{
		return 9.77 * pow(abs(omega / peakOmega), -2.33 - 1.45 * (u * peakOmega / g - 1.17));
	}
	else
	{
		return 6.97 * pow(abs(omega / peakOmega), 4.06);
	}
}

float Cosine2s(float theta, float s)
{
	return NormalisationFactor(s) * pow(abs(cos(0.5 * theta)), 2 * s);
}


float DirectionSpectrum(float theta, float omega, float peakOmega, SpectrumParams pars)
{
	float s = SpreadPowerHasselman(omega, peakOmega, pars.windSpeed)
			+ lerp(16 * tanh(min(omega / peakOmega / 10, 20)), 25, pars.extraAlignment) * pars.extraAlignment * pars.extraAlignment;
	float spread = Cosine2s(theta, s);
	
    return lerp(0.5 / OCEANOGRAPHY_PI, spread, pars.alignment);
}

//---------------------------------
// Energy spectrums
//---------------------------------

// Pierson-Moskowitz
float PiersonMoskowitzPeakOmega(float u)
{
	float nu = 0.13;
    return 2 * OCEANOGRAPHY_PI * nu * g / u;
}

float PiersonMoskowitz(float omega, float peakOmega)
{
	float oneOverOmega = 1 / omega;
	float peakOmegaOverOmega = peakOmega / omega;
	
	return 8.1e-3 * g * g * oneOverOmega * oneOverOmega * oneOverOmega * oneOverOmega * oneOverOmega
		* exp(-1.25 * peakOmegaOverOmega * peakOmegaOverOmega * peakOmegaOverOmega * peakOmegaOverOmega);
}

// JONSWAP
float JonswapAlpha(float chi)
{
	return 0.076 * pow(chi, -0.22);
}

float JonswapPeakOmega(float chi, float u)
{
	float nu = 3.5 * pow(chi, -0.33);
    return 2 * OCEANOGRAPHY_PI * nu * g / u;
}

float JONSWAP(float omega, float peakOmega, float chi, float gamma)
{
	float sigma;
	if (omega <= peakOmega)
		sigma = 0.07;
	else
		sigma = 0.09;
	
	float r = exp(-(omega - peakOmega) * (omega - peakOmega)
		/ 2 / sigma / sigma / peakOmega / peakOmega);
	
	float oneOverOmega = 1 / omega;
	float peakOmegaOverOmega = peakOmega / omega;
	return JonswapAlpha(chi) * g * g
		* oneOverOmega * oneOverOmega * oneOverOmega * oneOverOmega * oneOverOmega
		* exp(-1.25 * peakOmegaOverOmega * peakOmegaOverOmega * peakOmegaOverOmega * peakOmegaOverOmega)
		* pow(abs(gamma), r) * 3.3 / gamma;
}

float TMACorrection(float omega, float depth)
{
	float omegaH = omega * sqrt(depth / g);
	if (omegaH <= 1)
		return 0.5 * omegaH * omegaH;
	else if (omegaH < 2)
		return 1.0 - 0.5 * (2.0 - omegaH) * (2.0 - omegaH);
	else 
		return 1;
}

float FullSpectrum(float omega, float theta, SpectrumParams pars)
{
	float energySpectrum = 1;
	float spread;
	float peakOmega = 1;
	
	float chi = abs(g * pars.fetch * 1000 / pars.windSpeed / pars.windSpeed);
	chi = min(1e4, chi);
	
	if (pars.energySpectrum == 0)
	{
		peakOmega = PiersonMoskowitzPeakOmega(pars.windSpeed);
		energySpectrum = PiersonMoskowitz(omega, peakOmega);
	}
	
	if (pars.energySpectrum == 1)
	{
		peakOmega = JonswapPeakOmega(chi, pars.windSpeed);
		energySpectrum = JONSWAP(omega, peakOmega, chi, pars.peaking);
	}
	
	if (pars.energySpectrum == 2)
	{
		peakOmega = JonswapPeakOmega(chi, pars.windSpeed);
		energySpectrum = JONSWAP(omega, peakOmega, chi, pars.peaking) * TMACorrection(omega, Depth);
	}
	
	spread = DirectionSpectrum(theta, omega, peakOmega, pars);
	
	return energySpectrum * spread;
}

float ShortWavesFade(float kLength, float fadeLength)
{
	return exp(-fadeLength * fadeLength * kLength * kLength);
}
#endif