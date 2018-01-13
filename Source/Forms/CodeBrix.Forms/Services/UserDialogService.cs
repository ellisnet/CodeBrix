/* Copyright 2018 Ellisnet - Jeremy Ellis (jeremy@ellisnet.com)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Splat;
using AcrDialogs = Acr.UserDialogs;

// ReSharper disable once CheckNamespace
namespace CodeBrix.Services
{
    /// <summary> A user dialog service. </summary>
    public class UserDialogService : IUserDialogService
    {
        #region Private conversion methods

        /// <summary> Gets alert configuration. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="config"> The configuration. </param>
        /// <returns> The alert configuration. </returns>
        private AcrDialogs.AlertConfig GetAlertConfig(UserDialogAlertConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config));}

            var result = new AcrDialogs.AlertConfig();

            if (config.OkText != null) { result.OkText = config.OkText; }
            if (config.Title != null) { result.Title = config.Title; }
            if (config.Message != null) { result.Message = config.Message; }
            if (config.AndroidStyleId != null) { result.AndroidStyleId = config.AndroidStyleId; }
            if (config.OnAction != null) { result.OnAction = config.OnAction; }

            return result;
        }

        /// <summary> Gets action sheet configuration. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="config"> The configuration. </param>
        /// <returns> The action sheet configuration. </returns>
        private AcrDialogs.ActionSheetConfig GetActionSheetConfig(UserDialogActionSheetConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var result = new AcrDialogs.ActionSheetConfig();

            if (config.Title != null) { result.Title = config.Title; }
            if (config.Message != null) { result.Message = config.Message; }
            if (config.Cancel != null) { result.Cancel = GetActionSheetOption(config.Cancel); }
            if (config.Destructive != null) { result.Destructive = GetActionSheetOption(config.Destructive); }
            if (config.Options != null) { result.Options = config.Options.Select(GetActionSheetOption).ToList(); }
            if (config.AndroidStyleId != null) { result.AndroidStyleId = config.AndroidStyleId; }
            if (config.UseBottomSheet != null) { result.UseBottomSheet = config.UseBottomSheet.Value; }
            if (config.ItemIcon != null) { result.ItemIcon = config.ItemIcon; }

            return result;
        }

        /// <summary> Gets action sheet option. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="option"> The option. </param>
        /// <returns> The action sheet option. </returns>
        private AcrDialogs.ActionSheetOption GetActionSheetOption(UserDialogActionSheetOption option)
        {
            if (option == null) { throw new ArgumentNullException(nameof(option)); }
            return new AcrDialogs.ActionSheetOption(option.Text, option.Action, option.ItemIcon);
        }

        /// <summary> Gets confirm configuration. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="config"> The configuration. </param>
        /// <returns> The confirm configuration. </returns>
        private AcrDialogs.ConfirmConfig GetConfirmConfig(UserDialogConfirmConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var result = new AcrDialogs.ConfirmConfig();

            if (config.Title != null) { result.Title = config.Title; }
            if (config.Message != null) { result.Message = config.Message; }
            if (config.AndroidStyleId != null) { result.AndroidStyleId = config.AndroidStyleId; }
            if (config.OnAction != null) { result.OnAction = config.OnAction; }
            if (config.OkText != null) { result.OkText = config.OkText; }
            if (config.CancelText != null) { result.CancelText = config.CancelText; }

            return result;
        }

        /// <summary> Gets date prompt configuration. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="config"> The configuration. </param>
        /// <returns> The date prompt configuration. </returns>
        private AcrDialogs.DatePromptConfig GetDatePromptConfig(UserDialogDatePromptConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var result = new AcrDialogs.DatePromptConfig();

            if (config.Title != null) { result.Title = config.Title; }
            if (config.OkText != null) { result.OkText = config.OkText; }
            if (config.CancelText != null) { result.CancelText = config.CancelText; }
            if (config.SelectedDate != null) { result.SelectedDate = config.SelectedDate; }
            if (config.UnspecifiedDateTimeKindReplacement != null) { result.UnspecifiedDateTimeKindReplacement = config.UnspecifiedDateTimeKindReplacement.Value; }
            if (config.OnAction != null) { result.OnAction = GetDatePromptResultAction(config.OnAction); }
            if (config.IsCancellable != null) { result.IsCancellable = config.IsCancellable.Value; }
            if (config.MinimumDate != null) { result.MinimumDate = config.MinimumDate; }
            if (config.MaximumDate != null) { result.MaximumDate = config.MaximumDate; }
            if (config.AndroidStyleId != null) { result.AndroidStyleId = config.AndroidStyleId; }

            return result;
        }

        /// <summary> Convert date prompt result. </summary>
        /// <param name="result"> The result. </param>
        /// <returns> The date converted prompt result. </returns>
        private UserDialogDatePromptResult ConvertDatePromptResult(AcrDialogs.DatePromptResult result)
        {
            return (result == null) ? null : new UserDialogDatePromptResult(result.Ok, result.SelectedDate);
        }

        /// <summary> Gets date prompt result action. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="action"> The action. </param>
        /// <returns> The date prompt result action. </returns>
        private Action<AcrDialogs.DatePromptResult> GetDatePromptResultAction(Action<UserDialogDatePromptResult> action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }
            return (result) => { action.Invoke(ConvertDatePromptResult(result)); };
        }

        /// <summary> Gets time prompt configuration. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="config"> The configuration. </param>
        /// <returns> The time prompt configuration. </returns>
        private AcrDialogs.TimePromptConfig GetTimePromptConfig(UserDialogTimePromptConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var result = new AcrDialogs.TimePromptConfig();

            if (config.Title != null) { result.Title = config.Title; }
            if (config.OkText != null) { result.OkText = config.OkText; }
            if (config.CancelText != null) { result.CancelText = config.CancelText; }
            if (config.Use24HourClock != null) { result.Use24HourClock = config.Use24HourClock; }
            if (config.SelectedTime != null) { result.SelectedTime = config.SelectedTime; }
            if (config.OnAction != null) { result.OnAction = GetTimePromptResultAction(config.OnAction); }
            if (config.IsCancellable != null) { result.IsCancellable = config.IsCancellable.Value; }
            if (config.MinimumMinutesTimeOfDay != null) { result.MinimumMinutesTimeOfDay = config.MinimumMinutesTimeOfDay; }
            if (config.MaximumMinutesTimeOfDay != null) { result.MaximumMinutesTimeOfDay = config.MaximumMinutesTimeOfDay; }
            if (config.MinuteInterval != null) { result.MinuteInterval = config.MinuteInterval.Value; }
            if (config.AndroidStyleId != null) { result.AndroidStyleId = config.AndroidStyleId; }

            return result;
        }

        /// <summary> Convert time prompt result. </summary>
        /// <param name="result"> The result. </param>
        /// <returns> The time converted prompt result. </returns>
        private UserDialogTimePromptResult ConvertTimePromptResult(AcrDialogs.TimePromptResult result)
        {
            return (result == null) ? null : new UserDialogTimePromptResult(result.Ok, result.SelectedTime);
        }

        /// <summary> Gets time prompt result action. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="action"> The action. </param>
        /// <returns> The time prompt result action. </returns>
        private Action<AcrDialogs.TimePromptResult> GetTimePromptResultAction(Action<UserDialogTimePromptResult> action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }
            return (result) => { action.Invoke(ConvertTimePromptResult(result)); };
        }

        /// <summary> Gets prompt configuration. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="config"> The configuration. </param>
        /// <returns> The prompt configuration. </returns>
        private AcrDialogs.PromptConfig GetPromptConfig(UserDialogPromptConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var result = new AcrDialogs.PromptConfig();

            if (config.Title != null) { result.Title = config.Title; }
            if (config.Message != null) { result.Message = config.Message; }
            if (config.OnAction != null) { result.OnAction = GetPromptResultAction(config.OnAction); }
            if (config.IsCancellable != null) { result.IsCancellable = config.IsCancellable.Value; }
            if (config.Text != null) { result.Text = config.Text; }
            if (config.OkText != null) { result.OkText = config.OkText; }
            if (config.CancelText != null) { result.CancelText = config.CancelText; }
            if (config.Placeholder != null) { result.Placeholder = config.Placeholder; }
            if (config.MaxLength != null) { result.MaxLength = config.MaxLength; }
            if (config.AndroidStyleId != null) { result.AndroidStyleId = config.AndroidStyleId; }
            if (config.InputType != null) { result.InputType = ConvertToAcrInputType(config.InputType.Value); }
            if (config.OnTextChanged != null) { result.OnTextChanged = GetPromptTextChangedArgsAction(config.OnTextChanged); }

            return result;
        }

        /// <summary> Gets prompt result action. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="action"> The action. </param>
        /// <returns> The prompt result action. </returns>
        private Action<AcrDialogs.PromptResult> GetPromptResultAction(Action<UserDialogPromptResult> action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }
            return (result) => { action.Invoke(ConvertPromptResult(result)); };
        }

        /// <summary> Gets prompt text changed arguments action. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="action"> The action. </param>
        /// <returns> The prompt text changed arguments action. </returns>
        private Action<AcrDialogs.PromptTextChangedArgs> GetPromptTextChangedArgsAction(Action<UserDialogPromptTextChangedArgs> action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }
            return (result) => { action.Invoke(ConvertPromptTextChangedArgs(result)); };
        }

        /// <summary> Convert prompt result. </summary>
        /// <param name="result"> The result. </param>
        /// <returns> The prompt converted result. </returns>
        private UserDialogPromptResult ConvertPromptResult(AcrDialogs.PromptResult result)
        {
            return (result == null) ? null : new UserDialogPromptResult(result.Ok, result.Text);
        }

        /// <summary> Convert prompt text changed arguments. </summary>
        /// <param name="args"> The arguments. </param>
        /// <returns> The prompt converted text changed arguments. </returns>
        private UserDialogPromptTextChangedArgs ConvertPromptTextChangedArgs(AcrDialogs.PromptTextChangedArgs args)
        {
            return (args == null) ? null : new UserDialogPromptTextChangedArgs{ IsValid = args.IsValid, Value = args.Value };
        }

        /// <summary> Converts an inputType to an acr input type. </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="inputType"> Type of the input. </param>
        /// <returns> The given data converted to an acr input type. </returns>
        private AcrDialogs.InputType ConvertToAcrInputType(UserDialogInputType inputType)
        {
            switch (inputType)
            {
                case UserDialogInputType.Default:
                    return AcrDialogs.InputType.Default;
                    //break;
                case UserDialogInputType.Email:
                    return AcrDialogs.InputType.Email;
                    //break;
                case UserDialogInputType.Name:
                    return AcrDialogs.InputType.Name;
                    //break;
                case UserDialogInputType.Number:
                    return AcrDialogs.InputType.Number;
                    //break;
                case UserDialogInputType.DecimalNumber:
                    return AcrDialogs.InputType.DecimalNumber;
                    //break;
                case UserDialogInputType.Password:
                    return AcrDialogs.InputType.Password;
                    //break;
                case UserDialogInputType.NumericPassword:
                    return AcrDialogs.InputType.NumericPassword;
                    //break;
                case UserDialogInputType.Phone:
                    return AcrDialogs.InputType.Phone;
                    //break;
                case UserDialogInputType.Url:
                    return AcrDialogs.InputType.Url;
                    //break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null);
            }
        }

        /// <summary> Gets login configuration. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="config"> The configuration. </param>
        /// <returns> The login configuration. </returns>
        private AcrDialogs.LoginConfig GetLoginConfig(UserDialogLoginConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var result = new AcrDialogs.LoginConfig();

            if (config.Title != null) { result.Title = config.Title; }
            if (config.Message != null) { result.Message = config.Message; }
            if (config.OkText != null) { result.OkText = config.OkText; }
            if (config.CancelText != null) { result.CancelText = config.CancelText; }
            if (config.LoginValue != null) { result.LoginValue = config.LoginValue; }
            if (config.LoginPlaceholder != null) { result.LoginPlaceholder = config.LoginPlaceholder; }
            if (config.PasswordPlaceholder != null) { result.PasswordPlaceholder = config.PasswordPlaceholder; }
            if (config.AndroidStyleId != null) { result.AndroidStyleId = config.AndroidStyleId; }
            if (config.OnAction != null) { result.OnAction = GetLoginResultAction(config.OnAction); }

            return result;
        }

        /// <summary> Convert login result. </summary>
        /// <param name="result"> The result. </param>
        /// <returns> The login converted result. </returns>
        private UserDialogLoginResult ConvertLoginResult(AcrDialogs.LoginResult result)
        {
            return (result == null) ? null : new UserDialogLoginResult(result.Ok, result.LoginText, result.Password);
        }

        /// <summary> Gets login result action. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="action"> The action. </param>
        /// <returns> The login result action. </returns>
        private Action<AcrDialogs.LoginResult> GetLoginResultAction(Action<UserDialogLoginResult> action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }
            return (result) => { action.Invoke(ConvertLoginResult(result)); };
        }

        /// <summary> Converts a maskType to an acr mask type. </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="maskType"> Type of the mask. </param>
        /// <returns> The given data converted to an acr mask type. </returns>
        private AcrDialogs.MaskType ConvertToAcrMaskType(UserDialogMaskType maskType)
        {
            switch (maskType)
            {
                case UserDialogMaskType.Black:
                    return AcrDialogs.MaskType.Black;
                    //break;
                case UserDialogMaskType.Gradient:
                    return AcrDialogs.MaskType.Gradient;
                    //break;
                case UserDialogMaskType.Clear:
                    return AcrDialogs.MaskType.Clear;
                    //break;
                case UserDialogMaskType.None:
                    return AcrDialogs.MaskType.None;
                    //break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(maskType), maskType, null);
            }
        }

        /// <summary> Gets progress dialog configuration. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="config"> The configuration. </param>
        /// <returns> The progress dialog configuration. </returns>
        private AcrDialogs.ProgressDialogConfig GetProgressDialogConfig(UserDialogProgressConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var result = new AcrDialogs.ProgressDialogConfig();

            if (config.CancelText != null) { result.CancelText = config.CancelText; }
            if (config.Title != null) { result.Title = config.Title; }
            if (config.AutoShow != null) { result.AutoShow = config.AutoShow.Value; }
            if (config.IsDeterministic != null) { result.IsDeterministic = config.IsDeterministic.Value; }
            if (config.MaskType != null) { result.MaskType = ConvertToAcrMaskType(config.MaskType.Value); }
            if (config.OnCancel != null) { result.OnCancel = config.OnCancel; }

            return result;
        }

        /// <summary> Gets toast configuration. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="config"> The configuration. </param>
        /// <returns> The toast configuration. </returns>
        private AcrDialogs.ToastConfig GetToastConfig(UserDialogToastConfig config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            var result = new AcrDialogs.ToastConfig(config.Message);

            if (config.MessageTextColor != null) { result.MessageTextColor = config.MessageTextColor; }
            if (config.BackgroundColor != null) { result.BackgroundColor = config.BackgroundColor; }
            if (config.Position != null) { result.Position = ConvertToAcrToastPosition(config.Position.Value); }
            if (config.Duration != null) { result.Duration = config.Duration.Value; }
            if (config.Action != null) { result.Action = GetToastAction(config.Action); }
            if (config.Icon != null) { result.Icon = config.Icon; }

            return result;
        }

        /// <summary> Converts a position to an acr toast position. </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <param name="position"> The position. </param>
        /// <returns> The given data converted to an acr toast position. </returns>
        private AcrDialogs.ToastPosition ConvertToAcrToastPosition(UserDialogToastPosition position)
        {
            switch (position)
            {
                case UserDialogToastPosition.Top:
                    return AcrDialogs.ToastPosition.Top;
                    //break;
                case UserDialogToastPosition.Bottom:
                    return AcrDialogs.ToastPosition.Bottom;
                    //break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }
        }

        /// <summary> Gets toast action. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="action"> The action. </param>
        /// <returns> The toast action. </returns>
        private AcrDialogs.ToastAction GetToastAction(UserDialogToastAction action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

            var result = new AcrDialogs.ToastAction();

            if (action.Text != null) { result.Text = action.Text; }
            if (action.TextColor != null) { result.TextColor = action.TextColor; }
            if (action.Action != null) { result.Action = action.Action; }

            return result;
        }

        #endregion

        #region Implementation of IUserDialogService

        /// <summary> Alerts. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="okText"> (Optional) The ok text. </param>
        /// <returns> An IDisposable. </returns>
        public IDisposable Alert(string message, string title = null, string okText = null)
        {
            return AcrDialogs.UserDialogs.Instance.Alert(message, title, okText);
        }

        /// <summary> Alerts the given configuration. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        public IDisposable Alert(UserDialogAlertConfig config)
        {
            return AcrDialogs.UserDialogs.Instance.Alert(GetAlertConfig(config));
        }

        /// <summary> Alert asynchronous. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="okText"> (Optional) The ok text. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result. </returns>
        public Task AlertAsync(string message, string title = null, string okText = null, CancellationToken? cancelToken = null)
        {
            return AcrDialogs.UserDialogs.Instance.AlertAsync(message, title, okText, cancelToken);
        }

        /// <summary> Alert asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result. </returns>
        public Task AlertAsync(UserDialogAlertConfig config, CancellationToken? cancelToken = null)
        {
            return AcrDialogs.UserDialogs.Instance.AlertAsync(GetAlertConfig(config));
        }

        /// <summary> Action sheet. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        public IDisposable ActionSheet(UserDialogActionSheetConfig config)
        {
            return AcrDialogs.UserDialogs.Instance.ActionSheet(GetActionSheetConfig(config));
        }

        /// <summary> Action sheet asynchronous. </summary>
        /// <param name="title"> The title. </param>
        /// <param name="cancel"> The cancel. </param>
        /// <param name="destructive"> The destructive. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <param name="buttons"> A variable-length parameters list containing buttons. </param>
        /// <returns> The asynchronous result that yields a string. </returns>
        public Task<string> ActionSheetAsync(string title, string cancel, string destructive, CancellationToken? cancelToken = null,
            params string[] buttons)
        {
            return AcrDialogs.UserDialogs.Instance.ActionSheetAsync(title, cancel, destructive, cancelToken, buttons);
        }

        /// <summary> Confirms the given configuration. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        public IDisposable Confirm(UserDialogConfirmConfig config)
        {
            return AcrDialogs.UserDialogs.Instance.Confirm(GetConfirmConfig(config));
        }

        /// <summary> Confirm asynchronous. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="okText"> (Optional) The ok text. </param>
        /// <param name="cancelText"> (Optional) The cancel text. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        public Task<bool> ConfirmAsync(string message, string title = null, string okText = null, string cancelText = null,
            CancellationToken? cancelToken = null)
        {
            return AcrDialogs.UserDialogs.Instance.ConfirmAsync(message, title, okText, cancelText, cancelToken);
        }

        /// <summary> Confirm asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        public Task<bool> ConfirmAsync(UserDialogConfirmConfig config, CancellationToken? cancelToken = null)
        {
            return AcrDialogs.UserDialogs.Instance.ConfirmAsync(GetConfirmConfig(config), cancelToken);
        }

        /// <summary> Date prompt. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        public IDisposable DatePrompt(UserDialogDatePromptConfig config)
        {
            return AcrDialogs.UserDialogs.Instance.DatePrompt(GetDatePromptConfig(config));
        }

        /// <summary> Date prompt asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogDatePromptResult. </returns>
        public async Task<UserDialogDatePromptResult> DatePromptAsync(UserDialogDatePromptConfig config, CancellationToken? cancelToken = null)
        {
            AcrDialogs.DatePromptResult result = await AcrDialogs.UserDialogs.Instance.DatePromptAsync(GetDatePromptConfig(config), cancelToken);
            return ConvertDatePromptResult(result);
        }

        /// <summary> Date prompt asynchronous. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="selectedDate"> (Optional) The selected date. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogDatePromptResult. </returns>
        public async Task<UserDialogDatePromptResult> DatePromptAsync(string title = null, DateTime? selectedDate = null, CancellationToken? cancelToken = null)
        {
            AcrDialogs.DatePromptResult result = await AcrDialogs.UserDialogs.Instance.DatePromptAsync(title, selectedDate, cancelToken);
            return ConvertDatePromptResult(result);
        }

        /// <summary> Time prompt. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        public IDisposable TimePrompt(UserDialogTimePromptConfig config)
        {
            return AcrDialogs.UserDialogs.Instance.TimePrompt(GetTimePromptConfig(config));
        }

        /// <summary> Time prompt asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogTimePromptResult. </returns>
        public async Task<UserDialogTimePromptResult> TimePromptAsync(UserDialogTimePromptConfig config, CancellationToken? cancelToken = null)
        {
            AcrDialogs.TimePromptResult result = await AcrDialogs.UserDialogs.Instance.TimePromptAsync(GetTimePromptConfig(config), cancelToken);
            return ConvertTimePromptResult(result);
        }

        /// <summary> Time prompt asynchronous. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="selectedTime"> (Optional) The selected time. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogTimePromptResult. </returns>
        public async Task<UserDialogTimePromptResult> TimePromptAsync(string title = null, TimeSpan? selectedTime = null, CancellationToken? cancelToken = null)
        {
            AcrDialogs.TimePromptResult result = await AcrDialogs.UserDialogs.Instance.TimePromptAsync(title, selectedTime, cancelToken);
            return ConvertTimePromptResult(result);
        }

        /// <summary> Prompts the given configuration. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        public IDisposable Prompt(UserDialogPromptConfig config)
        {
            return AcrDialogs.UserDialogs.Instance.Prompt(GetPromptConfig(config));
        }

        /// <summary> Prompt asynchronous. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="okText"> (Optional) The ok text. </param>
        /// <param name="cancelText"> (Optional) The cancel text. </param>
        /// <param name="placeholder"> (Optional) The placeholder. </param>
        /// <param name="inputType"> (Optional) Type of the input. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogPromptResult. </returns>
        public async Task<UserDialogPromptResult> PromptAsync(string message, string title = null, string okText = null, string cancelText = null,
            string placeholder = "", UserDialogInputType inputType = UserDialogInputType.Default,
            CancellationToken? cancelToken = null)
        {
            AcrDialogs.PromptResult result = await AcrDialogs.UserDialogs.Instance.PromptAsync(message, title, okText, cancelText, placeholder, 
                ConvertToAcrInputType(inputType), cancelToken);
            return ConvertPromptResult(result);
        }

        /// <summary> Prompt asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogPromptResult. </returns>
        public async Task<UserDialogPromptResult> PromptAsync(UserDialogPromptConfig config, CancellationToken? cancelToken = null)
        {
            AcrDialogs.PromptResult result = await AcrDialogs.UserDialogs.Instance.PromptAsync(GetPromptConfig(config), cancelToken);
            return ConvertPromptResult(result);
        }

        /// <summary> Login. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        public IDisposable Login(UserDialogLoginConfig config)
        {
            return AcrDialogs.UserDialogs.Instance.Login(GetLoginConfig(config));
        }

        /// <summary> Login asynchronous. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="message"> (Optional) The message. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogLoginResult. </returns>
        public async Task<UserDialogLoginResult> LoginAsync(string title = null, string message = null, CancellationToken? cancelToken = null)
        {
            AcrDialogs.LoginResult result = await AcrDialogs.UserDialogs.Instance.LoginAsync(title, message, cancelToken);
            return ConvertLoginResult(result);
        }

        /// <summary> Login asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogLoginResult. </returns>
        public async Task<UserDialogLoginResult> LoginAsync(UserDialogLoginConfig config, CancellationToken? cancelToken = null)
        {
            AcrDialogs.LoginResult result = await AcrDialogs.UserDialogs.Instance.LoginAsync(GetLoginConfig(config), cancelToken);
            return ConvertLoginResult(result);
        }

        /// <summary> Progress the given configuration. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IUserProgressDialog. </returns>
        public IUserProgressDialog Progress(UserDialogProgressConfig config)
        {
            return new ProgressDialog(AcrDialogs.UserDialogs.Instance.Progress(GetProgressDialogConfig(config)));
        }

        /// <summary> Loadings. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="onCancel"> (Optional) The on cancel. </param>
        /// <param name="cancelText"> (Optional) The cancel text. </param>
        /// <param name="show"> (Optional) True to show, false to hide. </param>
        /// <param name="maskType"> (Optional) Type of the mask. </param>
        /// <returns> An IUserProgressDialog. </returns>
        public IUserProgressDialog Loading(string title = null, Action onCancel = null, string cancelText = null, bool show = true,
            UserDialogMaskType? maskType = null)
        {
            return (maskType == null)
                ? new ProgressDialog(AcrDialogs.UserDialogs.Instance.Loading(title, onCancel, cancelText, show))
                : new ProgressDialog(AcrDialogs.UserDialogs.Instance.Loading(title, onCancel, cancelText, show, ConvertToAcrMaskType(maskType.Value)));
        }

        /// <summary> Progress. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="onCancel"> (Optional) The on cancel. </param>
        /// <param name="cancelText"> (Optional) The cancel text. </param>
        /// <param name="show"> (Optional) True to show, false to hide. </param>
        /// <param name="maskType"> (Optional) Type of the mask. </param>
        /// <returns> An IUserProgressDialog. </returns>
        public IUserProgressDialog Progress(string title = null, Action onCancel = null, string cancelText = null, bool show = true,
            UserDialogMaskType? maskType = null)
        {
            return (maskType == null)
                ? new ProgressDialog(AcrDialogs.UserDialogs.Instance.Progress(title, onCancel, cancelText, show))
                : new ProgressDialog(AcrDialogs.UserDialogs.Instance.Progress(title, onCancel, cancelText, show, ConvertToAcrMaskType(maskType.Value)));
        }

        /// <summary> Shows the loading. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="maskType"> (Optional) Type of the mask. </param>
        public void ShowLoading(string title = null, UserDialogMaskType? maskType = null)
        {
            if (maskType == null)
            {
                AcrDialogs.UserDialogs.Instance.ShowLoading(title);
            }
            else
            {
                AcrDialogs.UserDialogs.Instance.ShowLoading(title, ConvertToAcrMaskType(maskType.Value));
            }
        }

        /// <summary> Hides the loading. </summary>
        public void HideLoading()
        {
            AcrDialogs.UserDialogs.Instance.HideLoading();
        }

        /// <summary> Shows the image. </summary>
        /// <param name="image"> The image. </param>
        /// <param name="message"> The message. </param>
        /// <param name="timeoutMillis"> (Optional) The timeout millis. </param>
        public void ShowImage(IBitmap image, string message, int timeoutMillis = 2000)
        {
            AcrDialogs.UserDialogs.Instance.ShowImage(image, message, timeoutMillis);
        }

        /// <summary> Toasts. </summary>
        /// <param name="title"> The title. </param>
        /// <param name="dismissTimer"> (Optional) The dismiss timer. </param>
        /// <returns> An IDisposable. </returns>
        public IDisposable Toast(string title, TimeSpan? dismissTimer = null)
        {
            return AcrDialogs.UserDialogs.Instance.Toast(title, dismissTimer);
        }

        /// <summary> Toasts the given configuration. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        public IDisposable Toast(UserDialogToastConfig config)
        {
            return AcrDialogs.UserDialogs.Instance.Toast(GetToastConfig(config));
        }

        #endregion
    }

    /// <summary> Dialog for reporting task progress. </summary>
    public class ProgressDialog : IUserProgressDialog
    {
        /// <summary> The abstracted dialog. </summary>
        private AcrDialogs.IProgressDialog _abstractedDialog;

        /// <summary> Constructor. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="abstractedDialog"> The abstracted dialog. </param>
        public ProgressDialog(AcrDialogs.IProgressDialog abstractedDialog)
        {
            _abstractedDialog = abstractedDialog ?? throw new ArgumentNullException(nameof(abstractedDialog));
        }

        /// <summary> Gets or sets the title. </summary>
        /// <value> The title. </value>
        public string Title
        {
            get => _abstractedDialog.Title;
            set => _abstractedDialog.Title = value;
        }

        /// <summary> Gets or sets the percent complete. </summary>
        /// <value> The percent complete. </value>
        public int PercentComplete
        {
            get => _abstractedDialog.PercentComplete;
            set => _abstractedDialog.PercentComplete = value;
        }

        /// <summary> Gets or sets a value indicating whether this object is showing. </summary>
        /// <value> True if this object is showing, false if not. </value>
        public bool IsShowing => _abstractedDialog.IsShowing;

        /// <summary> Shows this object. </summary>
        public void Show()
        {
            _abstractedDialog.Show();
        }

        /// <summary> Hides this object. </summary>
        public void Hide()
        {
            _abstractedDialog.Hide();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        public void Dispose()
        {
            _abstractedDialog.Dispose();
            _abstractedDialog = null;
        }
    }
}
