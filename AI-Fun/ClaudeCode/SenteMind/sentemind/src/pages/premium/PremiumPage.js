import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { 
  Star, 
  Check, 
  Zap, 
  Crown, 
  Shield, 
  TrendingUp, 
  Brain, 
  Heart,
  Users,
  Award,
  ArrowRight,
  Sparkles
} from 'lucide-react';

const PremiumPage = () => {
  const [billingCycle, setBillingCycle] = useState('monthly'); // monthly, yearly

  const plans = {
    monthly: {
      basic: {
        name: 'Free',
        price: 0,
        period: 'Forever',
        description: 'Perfect for getting started with mental wellness',
        features: [
          'Access to 4 basic tools',
          'Limited daily sessions',
          'Basic progress tracking',
          'Community support'
        ],
        limitations: [
          'No advanced analytics',
          'No personalized insights',
          'Limited customization'
        ],
        cta: 'Get Started Free',
        popular: false
      },
      pro: {
        name: 'Premium',
        price: 19.99,
        period: 'per month',
        description: 'Unlock your full mental potential with advanced tools',
        features: [
          'All 6+ premium mental health tools',
          'Unlimited daily sessions',
          'Advanced progress analytics',
          'Personalized AI insights',
          'Custom training programs',
          'Priority customer support',
          'Offline mode access',
          'Export your data'
        ],
        cta: 'Start Premium Trial',
        popular: true,
        savings: null
      },
      lifetime: {
        name: 'Lifetime Access',
        price: 299.99,
        period: 'one-time payment',
        description: 'The best value for serious mental wellness enthusiasts',
        features: [
          'Everything in Premium',
          'Lifetime updates',
          'Exclusive beta features',
          'Personal wellness coach',
          '1-on-1 consultation calls',
          'Advanced biometric integration',
          'Custom tool requests',
          'Lifetime guarantee'
        ],
        cta: 'Get Lifetime Access',
        popular: false,
        savings: 'Save $940 vs monthly'
      }
    },
    yearly: {
      basic: {
        name: 'Free',
        price: 0,
        period: 'Forever',
        description: 'Perfect for getting started with mental wellness',
        features: [
          'Access to 4 basic tools',
          'Limited daily sessions',
          'Basic progress tracking',
          'Community support'
        ],
        limitations: [
          'No advanced analytics',
          'No personalized insights',
          'Limited customization'
        ],
        cta: 'Get Started Free',
        popular: false
      },
      pro: {
        name: 'Premium Annual',
        price: 199.99,
        period: 'per year',
        description: 'Get 2 months free with annual billing',
        features: [
          'All 6+ premium mental health tools',
          'Unlimited daily sessions',
          'Advanced progress analytics',
          'Personalized AI insights',
          'Custom training programs',
          'Priority customer support',
          'Offline mode access',
          'Export your data'
        ],
        cta: 'Start Annual Plan',
        popular: true,
        savings: 'Save $40 vs monthly'
      },
      lifetime: {
        name: 'Lifetime Access',
        price: 299.99,
        period: 'one-time payment',
        description: 'The best value for serious mental wellness enthusiasts',
        features: [
          'Everything in Premium',
          'Lifetime updates',
          'Exclusive beta features',
          'Personal wellness coach',
          '1-on-1 consultation calls',
          'Advanced biometric integration',
          'Custom tool requests',
          'Lifetime guarantee'
        ],
        cta: 'Get Lifetime Access',
        popular: false,
        savings: 'Save $940 vs monthly'
      }
    }
  };

  const premiumFeatures = [
    {
      icon: Brain,
      title: 'Advanced Cognitive Training',
      description: 'Access exclusive algorithms and personalized difficulty adjustment for maximum cognitive gains',
      premium: true
    },
    {
      icon: TrendingUp,
      title: 'Detailed Analytics & Insights',
      description: 'Comprehensive performance tracking with AI-powered insights and recommendations',
      premium: true
    },
    {
      icon: Heart,
      title: 'Mood & Stress Analysis',
      description: 'Advanced mood tracking with pattern recognition and trigger identification',
      premium: true
    },
    {
      icon: Shield,
      title: 'Biometric Integration',
      description: 'Connect with wearables for heart rate variability and sleep quality insights',
      premium: true
    },
    {
      icon: Users,
      title: 'Personal Wellness Coach',
      description: '1-on-1 sessions with certified mental health professionals',
      premium: true
    },
    {
      icon: Sparkles,
      title: 'Custom Programs',
      description: 'Personalized training programs based on your goals and progress',
      premium: true
    }
  ];

  const testimonials = [
    {
      name: 'Sarah Johnson',
      role: 'Software Engineer',
      avatar: '👩‍💻',
      quote: 'SenteMind Premium has genuinely transformed my focus and productivity. The dual n-back training improved my working memory dramatically.',
      rating: 5
    },
    {
      name: 'Dr. Michael Chen',
      role: 'Neuroscientist',
      avatar: '👨‍🔬',
      quote: 'As a researcher, I appreciate the scientific rigor behind these tools. The binaural beats and mindfulness features are exceptionally well-designed.',
      rating: 5
    },
    {
      name: 'Emma Rodriguez',
      role: 'Marketing Manager',
      avatar: '👩‍💼',
      quote: 'The mood tracking and insights helped me identify stress patterns I never noticed. My anxiety has decreased significantly in just 3 months.',
      rating: 5
    }
  ];

  const faqs = [
    {
      question: 'What makes SenteMind different from other mental health apps?',
      answer: 'SenteMind combines cutting-edge neuroscience research with practical tools. Our dual n-back training is clinically proven to improve working memory, and our binaural beats use precise frequencies for optimal brainwave entrainment.'
    },
    {
      question: 'Is there scientific evidence supporting these tools?',
      answer: 'Absolutely. Every tool is based on peer-reviewed research from institutions like Harvard, Stanford, and MIT. We provide citations and research summaries for transparency.'
    },
    {
      question: 'Can I cancel my subscription anytime?',
      answer: 'Yes, you can cancel anytime with no questions asked. You\'ll continue to have access until the end of your billing period.'
    },
    {
      question: 'What devices are supported?',
      answer: 'SenteMind works on all modern web browsers, iOS, and Android devices. Premium features include offline mode and biometric integration.'
    },
    {
      question: 'Do you offer refunds?',
      answer: 'We offer a 30-day money-back guarantee. If you\'re not satisfied, we\'ll refund your purchase completely.'
    }
  ];

  const getCurrentPlans = () => plans[billingCycle];

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto px-4 py-12">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          className="text-center mb-16"
        >
          <div className="inline-flex items-center space-x-2 bg-yellow-100 text-yellow-800 px-4 py-2 rounded-full text-sm font-medium mb-6">
            <Sparkles className="w-4 h-4" />
            <span>Transform Your Mental Wellness</span>
          </div>
          <h1 className="text-5xl md:text-6xl font-bold text-gray-800 mb-6">
            Unlock Your Mind's
            <br />
            <span className="text-gradient">Full Potential</span>
          </h1>
          <p className="text-xl md:text-2xl text-gray-600 max-w-3xl mx-auto leading-relaxed">
            Join thousands who have enhanced their cognitive performance, reduced stress, and improved their mental wellbeing with science-backed tools.
          </p>
        </motion.div>

        {/* Social Proof */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
          className="text-center mb-16"
        >
          <div className="flex items-center justify-center space-x-8 text-gray-600">
            <div className="flex items-center space-x-2">
              <Users className="w-5 h-5" />
              <span className="font-semibold">50,000+ Users</span>
            </div>
            <div className="flex items-center space-x-2">
              <Star className="w-5 h-5 text-yellow-500" />
              <span className="font-semibold">4.9/5 Rating</span>
            </div>
            <div className="flex items-center space-x-2">
              <Award className="w-5 h-5" />
              <span className="font-semibold">Clinically Validated</span>
            </div>
          </div>
        </motion.div>

        {/* Billing Toggle */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="flex justify-center mb-12"
        >
          <div className="bg-white p-1 rounded-lg shadow-sm border">
            <button
              onClick={() => setBillingCycle('monthly')}
              className={`px-6 py-2 rounded-md text-sm font-medium transition-colors ${
                billingCycle === 'monthly'
                  ? 'bg-primary-500 text-white'
                  : 'text-gray-600 hover:text-gray-800'
              }`}
            >
              Monthly
            </button>
            <button
              onClick={() => setBillingCycle('yearly')}
              className={`px-6 py-2 rounded-md text-sm font-medium transition-colors relative ${
                billingCycle === 'yearly'
                  ? 'bg-primary-500 text-white'
                  : 'text-gray-600 hover:text-gray-800'
              }`}
            >
              Yearly
              <span className="absolute -top-2 -right-2 bg-green-500 text-white text-xs px-2 py-0.5 rounded-full">
                Save 20%
              </span>
            </button>
          </div>
        </motion.div>

        {/* Pricing Plans */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-20"
        >
          {Object.entries(getCurrentPlans()).map(([key, plan]) => (
            <div
              key={key}
              className={`relative bg-white rounded-2xl shadow-lg overflow-hidden ${
                plan.popular ? 'ring-2 ring-primary-500 scale-105' : ''
              }`}
            >
              {plan.popular && (
                <div className="absolute top-0 left-0 right-0 bg-gradient-to-r from-primary-500 to-secondary-500 text-white text-center py-2 text-sm font-medium">
                  <Crown className="w-4 h-4 inline mr-1" />
                  Most Popular
                </div>
              )}
              
              <div className={`p-8 ${plan.popular ? 'pt-12' : ''}`}>
                <div className="text-center mb-8">
                  <h3 className="text-2xl font-bold text-gray-800 mb-2">{plan.name}</h3>
                  <p className="text-gray-600 mb-6">{plan.description}</p>
                  
                  <div className="mb-4">
                    <span className="text-5xl font-bold text-gray-800">
                      ${plan.price}
                    </span>
                    <span className="text-gray-600 ml-2">{plan.period}</span>
                  </div>
                  
                  {plan.savings && (
                    <div className="inline-block bg-green-100 text-green-800 px-3 py-1 rounded-full text-sm font-medium">
                      {plan.savings}
                    </div>
                  )}
                </div>

                <ul className="space-y-4 mb-8">
                  {plan.features.map((feature, index) => (
                    <li key={index} className="flex items-start">
                      <Check className="w-5 h-5 text-green-500 mr-3 mt-0.5 flex-shrink-0" />
                      <span className="text-gray-700">{feature}</span>
                    </li>
                  ))}
                  {plan.limitations && plan.limitations.map((limitation, index) => (
                    <li key={`limit-${index}`} className="flex items-start opacity-50">
                      <div className="w-5 h-5 mr-3 mt-0.5 flex-shrink-0 flex items-center justify-center">
                        <div className="w-3 h-3 bg-gray-300 rounded-full"></div>
                      </div>
                      <span className="text-gray-500 line-through">{limitation}</span>
                    </li>
                  ))}
                </ul>

                <button
                  className={`w-full py-4 px-6 rounded-lg font-semibold transition-all ${
                    plan.popular
                      ? 'bg-gradient-to-r from-primary-500 to-secondary-500 text-white hover:from-primary-600 hover:to-secondary-600 transform hover:scale-105'
                      : key === 'basic'
                      ? 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                      : 'bg-primary-500 text-white hover:bg-primary-600'
                  }`}
                >
                  {plan.cta}
                  <ArrowRight className="w-4 h-4 inline ml-2" />
                </button>
              </div>
            </div>
          ))}
        </motion.div>

        {/* Premium Features */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.6 }}
          className="mb-20"
        >
          <div className="text-center mb-12">
            <h2 className="text-4xl font-bold text-gray-800 mb-4">
              Premium Features That Make a Difference
            </h2>
            <p className="text-xl text-gray-600 max-w-3xl mx-auto">
              Unlock advanced tools and insights designed by neuroscientists and mental health professionals.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {premiumFeatures.map((feature, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.7 + index * 0.1 }}
                className="bg-white rounded-lg p-6 shadow-sm hover:shadow-lg transition-shadow"
              >
                <div className="w-12 h-12 bg-gradient-to-br from-primary-500 to-secondary-500 rounded-lg flex items-center justify-center mb-4">
                  <feature.icon className="w-6 h-6 text-white" />
                </div>
                <h3 className="text-xl font-bold text-gray-800 mb-3">{feature.title}</h3>
                <p className="text-gray-600 leading-relaxed">{feature.description}</p>
                {feature.premium && (
                  <div className="mt-3">
                    <span className="inline-flex items-center px-2 py-1 bg-yellow-100 text-yellow-800 text-xs font-medium rounded">
                      <Star className="w-3 h-3 mr-1" />
                      Premium Only
                    </span>
                  </div>
                )}
              </motion.div>
            ))}
          </div>
        </motion.div>

        {/* Testimonials */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.8 }}
          className="mb-20"
        >
          <div className="text-center mb-12">
            <h2 className="text-4xl font-bold text-gray-800 mb-4">
              What Our Users Say
            </h2>
            <p className="text-xl text-gray-600">
              Real stories from people who transformed their mental wellness.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {testimonials.map((testimonial, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.9 + index * 0.1 }}
                className="bg-white rounded-lg p-6 shadow-sm"
              >
                <div className="flex items-center mb-4">
                  {[...Array(testimonial.rating)].map((_, i) => (
                    <Star key={i} className="w-4 h-4 text-yellow-500 fill-current" />
                  ))}
                </div>
                <blockquote className="text-gray-700 mb-4 italic">
                  "{testimonial.quote}"
                </blockquote>
                <div className="flex items-center">
                  <div className="text-2xl mr-3">{testimonial.avatar}</div>
                  <div>
                    <div className="font-semibold text-gray-800">{testimonial.name}</div>
                    <div className="text-sm text-gray-600">{testimonial.role}</div>
                  </div>
                </div>
              </motion.div>
            ))}
          </div>
        </motion.div>

        {/* FAQ */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 1 }}
          className="mb-20"
        >
          <div className="text-center mb-12">
            <h2 className="text-4xl font-bold text-gray-800 mb-4">
              Frequently Asked Questions
            </h2>
          </div>

          <div className="max-w-3xl mx-auto space-y-6">
            {faqs.map((faq, index) => (
              <div key={index} className="bg-white rounded-lg p-6 shadow-sm">
                <h3 className="text-lg font-semibold text-gray-800 mb-3">
                  {faq.question}
                </h3>
                <p className="text-gray-600 leading-relaxed">
                  {faq.answer}
                </p>
              </div>
            ))}
          </div>
        </motion.div>

        {/* Final CTA */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 1.2 }}
          className="bg-gradient-to-r from-primary-600 to-secondary-600 rounded-2xl p-12 text-center text-white"
        >
          <Zap className="w-16 h-16 mx-auto mb-6 text-yellow-300" />
          <h2 className="text-4xl font-bold mb-4">
            Ready to Transform Your Mind?
          </h2>
          <p className="text-xl mb-8 text-primary-100 max-w-2xl mx-auto">
            Join thousands of users who have enhanced their cognitive performance and emotional wellbeing. Start your journey today with a 30-day money-back guarantee.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <button className="btn bg-white text-primary-600 hover:bg-gray-100 text-lg px-8 py-4">
              Start 7-Day Free Trial
            </button>
            <button className="btn border-2 border-white text-white hover:bg-white hover:text-primary-600 text-lg px-8 py-4">
              Try Free Version
            </button>
          </div>
          <p className="text-sm text-primary-200 mt-4">
            No credit card required • Cancel anytime • 30-day money-back guarantee
          </p>
        </motion.div>
      </div>
    </div>
  );
};

export default PremiumPage;