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
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

using Splat;

namespace CodeBrix.Services
{
    /// <summary> Interface for user dialog service. </summary>
    public interface IUserDialogService
    {
        /// <summary> Alerts. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="okText"> (Optional) The ok text. </param>
        /// <returns> An IDisposable. </returns>
        IDisposable Alert(string message, string title = null, string okText = null);

        /// <summary> Alerts the given configuration. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        IDisposable Alert(UserDialogAlertConfig config);

        /// <summary> Alert asynchronous. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="okText"> (Optional) The ok text. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result. </returns>
        Task AlertAsync(string message, string title = null, string okText = null, CancellationToken? cancelToken = null);

        /// <summary> Alert asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result. </returns>
        Task AlertAsync(UserDialogAlertConfig config, CancellationToken? cancelToken = null);

        /// <summary> Action sheet. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        IDisposable ActionSheet(UserDialogActionSheetConfig config);

        /// <summary> Action sheet asynchronous. </summary>
        /// <param name="title"> The title. </param>
        /// <param name="cancel"> The cancel. </param>
        /// <param name="destructive"> The destructive. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <param name="buttons"> A variable-length parameters list containing buttons. </param>
        /// <returns> The asynchronous result that yields a string. </returns>
        Task<string> ActionSheetAsync(string title, string cancel, string destructive, CancellationToken? cancelToken = null, params string[] buttons);

        /// <summary> Confirms the given configuration. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        IDisposable Confirm(UserDialogConfirmConfig config);

        /// <summary> Confirm asynchronous. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="okText"> (Optional) The ok text. </param>
        /// <param name="cancelText"> (Optional) The cancel text. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        Task<bool> ConfirmAsync(string message, string title = null, string okText = null, string cancelText = null, CancellationToken? cancelToken = null);

        /// <summary> Confirm asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns>
        /// The asynchronous result that yields true if it succeeds, false if it fails.
        /// </returns>
        Task<bool> ConfirmAsync(UserDialogConfirmConfig config, CancellationToken? cancelToken = null);

        /// <summary> Date prompt. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        IDisposable DatePrompt(UserDialogDatePromptConfig config);

        /// <summary> Date prompt asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogDatePromptResult. </returns>
        Task<UserDialogDatePromptResult> DatePromptAsync(UserDialogDatePromptConfig config, CancellationToken? cancelToken = null);

        /// <summary> Date prompt asynchronous. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="selectedDate"> (Optional) The selected date. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogDatePromptResult. </returns>
        Task<UserDialogDatePromptResult> DatePromptAsync(string title = null, DateTime? selectedDate = null, CancellationToken? cancelToken = null);

        /// <summary> Time prompt. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        IDisposable TimePrompt(UserDialogTimePromptConfig config);

        /// <summary> Time prompt asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogTimePromptResult. </returns>
        Task<UserDialogTimePromptResult> TimePromptAsync(UserDialogTimePromptConfig config, CancellationToken? cancelToken = null);

        /// <summary> Time prompt asynchronous. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="selectedTime"> (Optional) The selected time. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogTimePromptResult. </returns>
        Task<UserDialogTimePromptResult> TimePromptAsync(string title = null, TimeSpan? selectedTime = null, CancellationToken? cancelToken = null);

        /// <summary> Prompts the given configuration. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        IDisposable Prompt(UserDialogPromptConfig config);

        /// <summary> Prompt asynchronous. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="okText"> (Optional) The ok text. </param>
        /// <param name="cancelText"> (Optional) The cancel text. </param>
        /// <param name="placeholder"> (Optional) The placeholder. </param>
        /// <param name="inputType"> (Optional) Type of the input. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogPromptResult. </returns>
        Task<UserDialogPromptResult> PromptAsync(string message, string title = null, string okText = null, string cancelText = null, string placeholder = "", UserDialogInputType inputType = UserDialogInputType.Default, CancellationToken? cancelToken = null);

        /// <summary> Prompt asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogPromptResult. </returns>
        Task<UserDialogPromptResult> PromptAsync(UserDialogPromptConfig config, CancellationToken? cancelToken = null);

        /// <summary> Login. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        IDisposable Login(UserDialogLoginConfig config);

        /// <summary> Login asynchronous. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="message"> (Optional) The message. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogLoginResult. </returns>
        Task<UserDialogLoginResult> LoginAsync(string title = null, string message = null, CancellationToken? cancelToken = null);

        /// <summary> Login asynchronous. </summary>
        /// <param name="config"> The configuration. </param>
        /// <param name="cancelToken"> (Optional) The cancel token. </param>
        /// <returns> The asynchronous result that yields an UserDialogLoginResult. </returns>
        Task<UserDialogLoginResult> LoginAsync(UserDialogLoginConfig config, CancellationToken? cancelToken = null);

        /// <summary> Progress the given configuration. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IUserProgressDialog. </returns>
        IUserProgressDialog Progress(UserDialogProgressConfig config);

        /// <summary> Loadings. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="onCancel"> (Optional) The on cancel. </param>
        /// <param name="cancelText"> (Optional) The cancel text. </param>
        /// <param name="show"> (Optional) True to show, false to hide. </param>
        /// <param name="maskType"> (Optional) Type of the mask. </param>
        /// <returns> An IUserProgressDialog. </returns>
        IUserProgressDialog Loading(string title = null, Action onCancel = null, string cancelText = null, bool show = true, UserDialogMaskType? maskType = null);

        /// <summary> Progress. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="onCancel"> (Optional) The on cancel. </param>
        /// <param name="cancelText"> (Optional) The cancel text. </param>
        /// <param name="show"> (Optional) True to show, false to hide. </param>
        /// <param name="maskType"> (Optional) Type of the mask. </param>
        /// <returns> An IUserProgressDialog. </returns>
        IUserProgressDialog Progress(string title = null, Action onCancel = null, string cancelText = null, bool show = true, UserDialogMaskType? maskType = null);

        /// <summary> Shows the loading. </summary>
        /// <param name="title"> (Optional) The title. </param>
        /// <param name="maskType"> (Optional) Type of the mask. </param>
        void ShowLoading(string title = null, UserDialogMaskType? maskType = null);
        /// <summary> Hides the loading. </summary>
        void HideLoading();

        /// <summary> Shows the image. </summary>
        /// <param name="image"> The image. </param>
        /// <param name="message"> The message. </param>
        /// <param name="timeoutMillis"> (Optional) The timeout millis. </param>
        void ShowImage(IBitmap image, string message, int timeoutMillis = 2000);

        /// <summary> Toasts. </summary>
        /// <param name="title"> The title. </param>
        /// <param name="dismissTimer"> (Optional) The dismiss timer. </param>
        /// <returns> An IDisposable. </returns>
        IDisposable Toast(string title, TimeSpan? dismissTimer = null);

        /// <summary> Toasts the given configuration. </summary>
        /// <param name="config"> The configuration. </param>
        /// <returns> An IDisposable. </returns>
        IDisposable Toast(UserDialogToastConfig config);
    }

    /// <summary> Values that represent user dialog mask types. </summary>
    public enum UserDialogMaskType
    {
        /// <summary> An enum constant representing the black option. </summary>
        Black,
        /// <summary> An enum constant representing the gradient option. </summary>
        Gradient,
        /// <summary> An enum constant representing the clear option. </summary>
        Clear,
        /// <summary> An enum constant representing the none option. </summary>
        None,
    }

    /// <summary> Values that represent user dialog input types. </summary>
    public enum UserDialogInputType
    {
        /// <summary> An enum constant representing the default option. </summary>
        Default,
        /// <summary> An enum constant representing the email option. </summary>
        Email,
        /// <summary> An enum constant representing the name option. </summary>
        Name,
        /// <summary> An enum constant representing the number option. </summary>
        Number,
        /// <summary> An enum constant representing the decimal number option. </summary>
        DecimalNumber,
        /// <summary> An enum constant representing the password option. </summary>
        Password,
        /// <summary> An enum constant representing the numeric password option. </summary>
        NumericPassword,
        /// <summary> An enum constant representing the phone option. </summary>
        Phone,
        /// <summary> An enum constant representing the URL option. </summary>
        Url,
    }

    /// <summary> Values that represent user dialog toast positions. </summary>
    public enum UserDialogToastPosition
    {
        /// <summary> An enum constant representing the top option. </summary>
        Top,
        /// <summary> An enum constant representing the bottom option. </summary>
        Bottom,
    }

    /// <summary> Interface for user progress dialog. </summary>
    public interface IUserProgressDialog : IDisposable
    {
        /// <summary> Gets or sets the title. </summary>
        /// <value> The title. </value>
        string Title { get; set; }

        /// <summary> Gets or sets the percent complete. </summary>
        /// <value> The percent complete. </value>
        int PercentComplete { get; set; }

        /// <summary> Gets a value indicating whether this object is showing. </summary>
        /// <value> True if this object is showing, false if not. </value>
        bool IsShowing { get; }
        /// <summary> Shows this object. </summary>
        void Show();
        /// <summary> Hides this object. </summary>
        void Hide();
    }

    /// <summary> A user dialog alert configuration. </summary>
    public class UserDialogAlertConfig
    {
        /// <summary> Gets or sets the ok text. </summary>
        /// <value> The ok text. </value>
        public string OkText { get; set; }

        /// <summary> Gets or sets the title. </summary>
        /// <value> The title. </value>
        public string Title { get; set; }

        /// <summary> Gets or sets the message. </summary>
        /// <value> The message. </value>
        public string Message { get; set; }

        /// <summary> Gets or sets the identifier of the android style. </summary>
        /// <value> The identifier of the android style. </value>
        public int? AndroidStyleId { get; set; }

        /// <summary> Gets or sets the on action. </summary>
        /// <value> The on action. </value>
        public Action OnAction { get; set; }

        /// <summary> Sets ok text. </summary>
        /// <param name="text"> The text. </param>
        /// <returns> An UserDialogAlertConfig. </returns>
        public UserDialogAlertConfig SetOkText(string text)
        {
            OkText = text;
            return this;
        }

        /// <summary> Sets a title. </summary>
        /// <param name="title"> The title. </param>
        /// <returns> An UserDialogAlertConfig. </returns>
        public UserDialogAlertConfig SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary> Sets a message. </summary>
        /// <param name="message"> The message. </param>
        /// <returns> An UserDialogAlertConfig. </returns>
        public UserDialogAlertConfig SetMessage(string message)
        {
            Message = message;
            return this;
        }

        /// <summary> Sets an action. </summary>
        /// <param name="action"> The action. </param>
        /// <returns> An UserDialogAlertConfig. </returns>
        public UserDialogAlertConfig SetAction(Action action)
        {
            OnAction = action;
            return this;
        }
    }

    /// <summary> A user dialog action sheet configuration. </summary>
    public class UserDialogActionSheetConfig
    {
        /// <summary> Gets or sets the title. </summary>
        /// <value> The title. </value>
        public string Title { get; set; }

        /// <summary> Gets or sets the message. </summary>
        /// <value> The message. </value>
        public string Message { get; set; }

        /// <summary> Gets or sets the cancel. </summary>
        /// <value> The cancel. </value>
        public UserDialogActionSheetOption Cancel { get; set; }

        /// <summary> Gets or sets the destructive. </summary>
        /// <value> The destructive. </value>
        public UserDialogActionSheetOption Destructive { get; set; }

        /// <summary> Gets or sets options for controlling the operation. </summary>
        /// <value> The options. </value>
        public IList<UserDialogActionSheetOption> Options { get; set; }

        /// <summary> Gets or sets the identifier of the android style. </summary>
        /// <value> The identifier of the android style. </value>
        public int? AndroidStyleId { get; set; }

        /// <summary> Gets or sets the use bottom sheet. </summary>
        /// <value> The use bottom sheet. </value>
        public bool? UseBottomSheet { get; set; }

        /// <summary> Gets or sets the item icon. </summary>
        /// <value> The item icon. </value>
        public IBitmap ItemIcon { get; set; }

        /// <summary> Sets a title. </summary>
        /// <param name="title"> The title. </param>
        /// <returns> An UserDialogActionSheetConfig. </returns>
        public UserDialogActionSheetConfig SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary> Sets use bottom sheet. </summary>
        /// <param name="useBottomSheet"> True to use bottom sheet. </param>
        /// <returns> An UserDialogActionSheetConfig. </returns>
        public UserDialogActionSheetConfig SetUseBottomSheet(bool useBottomSheet)
        {
            UseBottomSheet = useBottomSheet;
            return this;
        }

        /// <summary> Sets a cancel. </summary>
        /// <param name="text"> (Optional) The text. </param>
        /// <param name="action"> (Optional) The action. </param>
        /// <param name="icon"> (Optional) The icon. </param>
        /// <returns> An UserDialogActionSheetConfig. </returns>
        public UserDialogActionSheetConfig SetCancel(string text = null, Action action = null, IBitmap icon = null)
        {
            Cancel = new UserDialogActionSheetOption(text, action, icon);
            return this;
        }

        /// <summary> Sets a destructive. </summary>
        /// <param name="text"> (Optional) The text. </param>
        /// <param name="action"> (Optional) The action. </param>
        /// <param name="icon"> (Optional) The icon. </param>
        /// <returns> An UserDialogActionSheetConfig. </returns>
        public UserDialogActionSheetConfig SetDestructive(string text = null, Action action = null, IBitmap icon = null)
        {
            Destructive = new UserDialogActionSheetOption(text, action, icon);
            return this;
        }

        /// <summary> Sets a message. </summary>
        /// <param name="msg"> The message. </param>
        /// <returns> An UserDialogActionSheetConfig. </returns>
        public UserDialogActionSheetConfig SetMessage(string msg)
        {
            Message = msg;
            return this;
        }

        /// <summary> Adds text. </summary>
        /// <param name="text"> The text. </param>
        /// <param name="action"> (Optional) The action. </param>
        /// <param name="icon"> (Optional) The icon. </param>
        /// <returns> An UserDialogActionSheetConfig. </returns>
        public UserDialogActionSheetConfig Add(string text, Action action = null, IBitmap icon = null)
        {
            Options = Options ?? new List<UserDialogActionSheetOption>();
            Options.Add(new UserDialogActionSheetOption(text, action, icon));
            return this;
        }
    }

    /// <summary> A user dialog action sheet option. </summary>
    public class UserDialogActionSheetOption
    {
        /// <summary> Gets or sets the text. </summary>
        /// <value> The text. </value>
        public string Text { get; set; }

        /// <summary> Gets or sets the action. </summary>
        /// <value> The action. </value>
        public Action Action { get; set; }

        /// <summary> Gets or sets the item icon. </summary>
        /// <value> The item icon. </value>
        public IBitmap ItemIcon { get; set; }

        /// <summary> Constructor. </summary>
        /// <param name="text"> The text. </param>
        /// <param name="action"> (Optional) The action. </param>
        /// <param name="icon"> (Optional) The icon. </param>
        public UserDialogActionSheetOption(string text, Action action = null, IBitmap icon = null)
        {
            Text = text;
            Action = action;
            ItemIcon = icon;
        }
    }

    /// <summary> A user dialog confirm configuration. </summary>
    public class UserDialogConfirmConfig
    {
        /// <summary> Gets or sets the title. </summary>
        /// <value> The title. </value>
        public string Title { get; set; }

        /// <summary> Gets or sets the message. </summary>
        /// <value> The message. </value>
        public string Message { get; set; }

        /// <summary> Gets or sets the identifier of the android style. </summary>
        /// <value> The identifier of the android style. </value>
        public int? AndroidStyleId { get; set; }

        /// <summary> Gets or sets the on action. </summary>
        /// <value> The on action. </value>
        public Action<bool> OnAction { get; set; }

        /// <summary> Gets or sets the ok text. </summary>
        /// <value> The ok text. </value>
        public string OkText { get; set; }

        /// <summary> Gets or sets the cancel text. </summary>
        /// <value> The cancel text. </value>
        public string CancelText { get; set; }

        /// <summary> Use yes no. </summary>
        /// <returns> An UserDialogConfirmConfig. </returns>
        public UserDialogConfirmConfig UseYesNo()
        {
            OkText = "Yes";
            CancelText = "No";
            return this;
        }

        /// <summary> Sets a title. </summary>
        /// <param name="title"> The title. </param>
        /// <returns> An UserDialogConfirmConfig. </returns>
        public UserDialogConfirmConfig SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary> Sets a message. </summary>
        /// <param name="message"> The message. </param>
        /// <returns> An UserDialogConfirmConfig. </returns>
        public UserDialogConfirmConfig SetMessage(string message)
        {
            Message = message;
            return this;
        }

        /// <summary> Sets ok text. </summary>
        /// <param name="text"> The text. </param>
        /// <returns> An UserDialogConfirmConfig. </returns>
        public UserDialogConfirmConfig SetOkText(string text)
        {
            OkText = text;
            return this;
        }

        /// <summary> Sets an action. </summary>
        /// <param name="action"> The action. </param>
        /// <returns> An UserDialogConfirmConfig. </returns>
        public UserDialogConfirmConfig SetAction(Action<bool> action)
        {
            OnAction = action;
            return this;
        }

        /// <summary> Sets cancel text. </summary>
        /// <param name="text"> The text. </param>
        /// <returns> An UserDialogConfirmConfig. </returns>
        public UserDialogConfirmConfig SetCancelText(string text)
        {
            CancelText = text;
            return this;
        }
    }

    /// <summary> A user dialog date prompt configuration. </summary>
    public class UserDialogDatePromptConfig
    {
        /// <summary> Gets or sets the title. </summary>
        /// <value> The title. </value>
        public string Title { get; set; }

        /// <summary> Gets or sets the ok text. </summary>
        /// <value> The ok text. </value>
        public string OkText { get; set; }

        /// <summary> Gets or sets the cancel text. </summary>
        /// <value> The cancel text. </value>
        public string CancelText { get; set; }

        /// <summary> Gets or sets the selected date. </summary>
        /// <value> The selected date. </value>
        public DateTime? SelectedDate { get; set; }

        /// <summary> Gets or sets the unspecified date time kind replacement. </summary>
        /// <value> The unspecified date time kind replacement. </value>
        public DateTimeKind? UnspecifiedDateTimeKindReplacement { get; set; }

        /// <summary> Gets or sets the on action. </summary>
        /// <value> The on action. </value>
        public Action<UserDialogDatePromptResult> OnAction { get; set; }

        /// <summary> Gets or sets the is cancellable. </summary>
        /// <value> The is cancellable. </value>
        public bool? IsCancellable { get; set; }

        /// <summary> Gets or sets the minimum date. </summary>
        /// <value> The minimum date. </value>
        public DateTime? MinimumDate { get; set; }

        /// <summary> Gets or sets the maximum date. </summary>
        /// <value> The maximum date. </value>
        public DateTime? MaximumDate { get; set; }

        /// <summary> Gets or sets the identifier of the android style. </summary>
        /// <value> The identifier of the android style. </value>
        public int? AndroidStyleId { get; set; }
    }

    /// <summary> Encapsulates the result of a user dialog date prompt. </summary>
    public class UserDialogDatePromptResult
    {
        /// <summary> Constructor. </summary>
        /// <param name="ok"> True if ok, false if not. </param>
        /// <param name="selectedDate"> The selected date. </param>
        public UserDialogDatePromptResult(bool ok, DateTime selectedDate)
        {
            Ok = ok;
            SelectedDate = selectedDate;
        }

        /// <summary> Gets the selected date. </summary>
        /// <value> The selected date. </value>
        public DateTime SelectedDate { get; }

        /// <summary> Gets a value indicating whether the ok. </summary>
        /// <value> True if ok, false if not. </value>
        public bool Ok { get; }
    }

    /// <summary> A user dialog time prompt configuration. </summary>
    public class UserDialogTimePromptConfig
    {
        /// <summary> Gets or sets the title. </summary>
        /// <value> The title. </value>
        public string Title { get; set; }

        /// <summary> Gets or sets the ok text. </summary>
        /// <value> The ok text. </value>
        public string OkText { get; set; }

        /// <summary> Gets or sets the cancel text. </summary>
        /// <value> The cancel text. </value>
        public string CancelText { get; set; }

        /// <summary> Gets or sets the use 24 hour clock. </summary>
        /// <value> The use 24 hour clock. </value>
        public bool? Use24HourClock { get; set; }

        /// <summary> Gets or sets the selected time. </summary>
        /// <value> The selected time. </value>
        public TimeSpan? SelectedTime { get; set; }

        /// <summary> Gets or sets the on action. </summary>
        /// <value> The on action. </value>
        public Action<UserDialogTimePromptResult> OnAction { get; set; }

        /// <summary> Gets or sets the is cancellable. </summary>
        /// <value> The is cancellable. </value>
        public bool? IsCancellable { get; set; }

        /// <summary> Gets or sets the minimum minutes time of day. </summary>
        /// <value> The minimum minutes time of day. </value>
        public int? MinimumMinutesTimeOfDay { get; set; }

        /// <summary> Gets or sets the maximum minutes time of day. </summary>
        /// <value> The maximum minutes time of day. </value>
        public int? MaximumMinutesTimeOfDay { get; set; }

        /// <summary> Gets or sets the minute interval. </summary>
        /// <value> The minute interval. </value>
        public int? MinuteInterval { get; set; }

        /// <summary> Gets or sets the identifier of the android style. </summary>
        /// <value> The identifier of the android style. </value>
        public int? AndroidStyleId { get; set; }
    }

    /// <summary> Encapsulates the result of a user dialog time prompt. </summary>
    public class UserDialogTimePromptResult
    {
        /// <summary> Constructor. </summary>
        /// <param name="ok"> True if ok, false if not. </param>
        /// <param name="selectedTime"> The selected time. </param>
        public UserDialogTimePromptResult(bool ok, TimeSpan selectedTime)
        {
            Ok = ok;
            SelectedTime = selectedTime;
        }

        /// <summary> Gets a value indicating whether the ok. </summary>
        /// <value> True if ok, false if not. </value>
        public bool Ok { get; }

        /// <summary> Gets the selected time. </summary>
        /// <value> The selected time. </value>
        public TimeSpan SelectedTime { get; }
    }

    /// <summary> A user dialog prompt configuration. </summary>
    public class UserDialogPromptConfig
    {
        /// <summary> Gets or sets the title. </summary>
        /// <value> The title. </value>
        public string Title { get; set; }

        /// <summary> Gets or sets the message. </summary>
        /// <value> The message. </value>
        public string Message { get; set; }

        /// <summary> Gets or sets the on action. </summary>
        /// <value> The on action. </value>
        public Action<UserDialogPromptResult> OnAction { get; set; }

        /// <summary> Gets or sets the is cancellable. </summary>
        /// <value> The is cancellable. </value>
        public bool? IsCancellable { get; set; }

        /// <summary> Gets or sets the text. </summary>
        /// <value> The text. </value>
        public string Text { get; set; }

        /// <summary> Gets or sets the ok text. </summary>
        /// <value> The ok text. </value>
        public string OkText { get; set; }

        /// <summary> Gets or sets the cancel text. </summary>
        /// <value> The cancel text. </value>
        public string CancelText { get; set; }

        /// <summary> Gets or sets the placeholder. </summary>
        /// <value> The placeholder. </value>
        public string Placeholder { get; set; }

        /// <summary> Gets or sets the length of the maximum. </summary>
        /// <value> The length of the maximum. </value>
        public int? MaxLength { get; set; }

        /// <summary> Gets or sets the identifier of the android style. </summary>
        /// <value> The identifier of the android style. </value>
        public int? AndroidStyleId { get; set; }

        /// <summary> Gets or sets the type of the input. </summary>
        /// <value> The type of the input. </value>
        public UserDialogInputType? InputType { get; set; }

        /// <summary> Gets or sets the on text changed. </summary>
        /// <value> The on text changed. </value>
        public Action<UserDialogPromptTextChangedArgs> OnTextChanged { get; set; }

        /// <summary> Sets an action. </summary>
        /// <param name="action"> The action. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetAction(Action<UserDialogPromptResult> action)
        {
            OnAction = action;
            return this;
        }

        /// <summary> Sets a title. </summary>
        /// <param name="title"> The title. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary> Sets a message. </summary>
        /// <param name="message"> The message. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetMessage(string message)
        {
            Message = message;
            return this;
        }

        /// <summary> Sets a cancellable. </summary>
        /// <param name="cancel"> True to cancel. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetCancellable(bool cancel)
        {
            IsCancellable = cancel;
            return this;
        }

        /// <summary> Sets ok text. </summary>
        /// <param name="text"> The text. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetOkText(string text)
        {
            OkText = text;
            return this;
        }

        /// <summary> Sets maximum length. </summary>
        /// <param name="maxLength"> The maximum length. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetMaxLength(int maxLength)
        {
            MaxLength = maxLength;
            return this;
        }

        /// <summary> Sets a text. </summary>
        /// <param name="text"> The text. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary> Sets cancel text. </summary>
        /// <param name="cancelText"> The cancel text. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetCancelText(string cancelText)
        {
            CancelText = cancelText;
            return this;
        }

        /// <summary> Sets a placeholder. </summary>
        /// <param name="placeholder"> The placeholder. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }

        /// <summary> Sets input mode. </summary>
        /// <param name="inputType"> Type of the input. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetInputMode(UserDialogInputType inputType)
        {
            InputType = inputType;
            return this;
        }

        /// <summary> Sets on text changed. </summary>
        /// <param name="onChange"> The on change. </param>
        /// <returns> An UserDialogPromptConfig. </returns>
        public UserDialogPromptConfig SetOnTextChanged(Action<UserDialogPromptTextChangedArgs> onChange)
        {
            OnTextChanged = onChange;
            return this;
        }
    }

    /// <summary> Encapsulates the result of a user dialog prompt. </summary>
    public class UserDialogPromptResult
    {
        /// <summary> Constructor. </summary>
        /// <param name="ok"> True if ok, false if not. </param>
        /// <param name="text"> The text. </param>
        public UserDialogPromptResult(bool ok, string text)
        {
            Ok = ok;
            Text = text;
        }

        /// <summary> Gets a value indicating whether the ok. </summary>
        /// <value> True if ok, false if not. </value>
        public bool Ok { get; }

        /// <summary> Gets the text. </summary>
        /// <value> The text. </value>
        public string Text { get; }
    }

    /// <summary> Arguments for user dialog prompt text changed. </summary>
    public class UserDialogPromptTextChangedArgs
    {
        /// <summary> Gets or sets a value indicating whether this object is valid. </summary>
        /// <value> True if this object is valid, false if not. </value>
        public bool IsValid { get; set; }

        /// <summary> Gets or sets the value. </summary>
        /// <value> The value. </value>
        public string Value { get; set; }
    }

    /// <summary> A user dialog login configuration. </summary>
    public class UserDialogLoginConfig
    {
        /// <summary> Gets or sets the title. </summary>
        /// <value> The title. </value>
        public string Title { get; set; }

        /// <summary> Gets or sets the message. </summary>
        /// <value> The message. </value>
        public string Message { get; set; }

        /// <summary> Gets or sets the ok text. </summary>
        /// <value> The ok text. </value>
        public string OkText { get; set; }

        /// <summary> Gets or sets the cancel text. </summary>
        /// <value> The cancel text. </value>
        public string CancelText { get; set; }

        /// <summary> Gets or sets the login value. </summary>
        /// <value> The login value. </value>
        public string LoginValue { get; set; }

        /// <summary> Gets or sets the login placeholder. </summary>
        /// <value> The login placeholder. </value>
        public string LoginPlaceholder { get; set; }

        /// <summary> Gets or sets the password placeholder. </summary>
        /// <value> The password placeholder. </value>
        public string PasswordPlaceholder { get; set; }

        /// <summary> Gets or sets the identifier of the android style. </summary>
        /// <value> The identifier of the android style. </value>
        public int? AndroidStyleId { get; set; }

        /// <summary> Gets or sets the on action. </summary>
        /// <value> The on action. </value>
        public Action<UserDialogLoginResult> OnAction { get; set; }

        /// <summary> Sets a title. </summary>
        /// <param name="title"> The title. </param>
        /// <returns> An UserDialogLoginConfig. </returns>
        public UserDialogLoginConfig SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary> Sets a message. </summary>
        /// <param name="msg"> The message. </param>
        /// <returns> An UserDialogLoginConfig. </returns>
        public UserDialogLoginConfig SetMessage(string msg)
        {
            Message = msg;
            return this;
        }

        /// <summary> Sets ok text. </summary>
        /// <param name="ok"> The ok. </param>
        /// <returns> An UserDialogLoginConfig. </returns>
        public UserDialogLoginConfig SetOkText(string ok)
        {
            OkText = ok;
            return this;
        }

        /// <summary> Sets cancel text. </summary>
        /// <param name="cancel"> The cancel. </param>
        /// <returns> An UserDialogLoginConfig. </returns>
        public UserDialogLoginConfig SetCancelText(string cancel)
        {
            CancelText = cancel;
            return this;
        }

        /// <summary> Sets login value. </summary>
        /// <param name="txt"> The text. </param>
        /// <returns> An UserDialogLoginConfig. </returns>
        public UserDialogLoginConfig SetLoginValue(string txt)
        {
            LoginValue = txt;
            return this;
        }

        /// <summary> Sets login placeholder. </summary>
        /// <param name="txt"> The text. </param>
        /// <returns> An UserDialogLoginConfig. </returns>
        public UserDialogLoginConfig SetLoginPlaceholder(string txt)
        {
            LoginPlaceholder = txt;
            return this;
        }

        /// <summary> Sets password placeholder. </summary>
        /// <param name="txt"> The text. </param>
        /// <returns> An UserDialogLoginConfig. </returns>
        public UserDialogLoginConfig SetPasswordPlaceholder(string txt)
        {
            PasswordPlaceholder = txt;
            return this;
        }

        /// <summary> Sets an action. </summary>
        /// <param name="action"> The action. </param>
        /// <returns> An UserDialogLoginConfig. </returns>
        public UserDialogLoginConfig SetAction(Action<UserDialogLoginResult> action)
        {
            OnAction = action;
            return this;
        }
    }

    /// <summary> Encapsulates the result of a user dialog login. </summary>
    public class UserDialogLoginResult
    {
        /// <summary> Gets the login text. </summary>
        /// <value> The login text. </value>
        public string LoginText { get; }

        /// <summary> Gets the password. </summary>
        /// <value> The password. </value>
        public string Password { get; }

        /// <summary> Gets a value indicating whether the ok. </summary>
        /// <value> True if ok, false if not. </value>
        public bool Ok { get; }

        /// <summary> Constructor. </summary>
        /// <param name="ok"> True if the operation was a success, false if it failed. </param>
        /// <param name="login"> The login. </param>
        /// <param name="pass"> The pass. </param>
        public UserDialogLoginResult(bool ok, string login, string pass)
        {
            Ok = ok;
            LoginText = login;
            Password = pass;
        }
    }

    /// <summary> A user dialog progress configuration. </summary>
    public class UserDialogProgressConfig
    {
        /// <summary> Gets or sets the cancel text. </summary>
        /// <value> The cancel text. </value>
        public string CancelText { get; set; }

        /// <summary> Gets or sets the title. </summary>
        /// <value> The title. </value>
        public string Title { get; set; }

        /// <summary> Gets or sets the automatic show. </summary>
        /// <value> The automatic show. </value>
        public bool? AutoShow { get; set; }

        /// <summary> Gets or sets the is deterministic. </summary>
        /// <value> The is deterministic. </value>
        public bool? IsDeterministic { get; set; }

        /// <summary> Gets or sets the type of the mask. </summary>
        /// <value> The type of the mask. </value>
        public UserDialogMaskType? MaskType { get; set; }

        /// <summary> Gets or sets the on cancel. </summary>
        /// <value> The on cancel. </value>
        public Action OnCancel { get; set; }

        /// <summary> Sets a cancel. </summary>
        /// <param name="cancelText"> (Optional) The cancel text. </param>
        /// <param name="onCancel"> (Optional) The on cancel. </param>
        /// <returns> An UserDialogProgressConfig. </returns>
        public UserDialogProgressConfig SetCancel(string cancelText = null, Action onCancel = null)
        {
            CancelText = cancelText;
            OnCancel = onCancel;
            return this;
        }

        /// <summary> Sets a title. </summary>
        /// <param name="title"> The title. </param>
        /// <returns> An UserDialogProgressConfig. </returns>
        public UserDialogProgressConfig SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary> Sets mask type. </summary>
        /// <param name="maskType"> Type of the mask. </param>
        /// <returns> An UserDialogProgressConfig. </returns>
        public UserDialogProgressConfig SetMaskType(UserDialogMaskType maskType)
        {
            MaskType = maskType;
            return this;
        }

        /// <summary> Sets automatic show. </summary>
        /// <param name="autoShow"> True to automatically show. </param>
        /// <returns> An UserDialogProgressConfig. </returns>
        public UserDialogProgressConfig SetAutoShow(bool autoShow)
        {
            AutoShow = autoShow;
            return this;
        }

        /// <summary> Sets is deterministic. </summary>
        /// <param name="isDeterministic"> True if this object is deterministic. </param>
        /// <returns> An UserDialogProgressConfig. </returns>
        public UserDialogProgressConfig SetIsDeterministic(bool isDeterministic)
        {
            IsDeterministic = isDeterministic;
            return this;
        }
    }

    /// <summary> A user dialog toast configuration. </summary>
    public class UserDialogToastConfig
    {
        /// <summary> Gets or sets the message. </summary>
        /// <value> The message. </value>
        public string Message { get; set; }

        /// <summary> Gets or sets the color of the message text. </summary>
        /// <value> The color of the message text. </value>
        public Color? MessageTextColor { get; set; }

        /// <summary> Gets or sets the color of the background. </summary>
        /// <value> The color of the background. </value>
        public Color? BackgroundColor { get; set; }

        /// <summary> Gets or sets the position. </summary>
        /// <value> The position. </value>
        public UserDialogToastPosition? Position { get; set; }

        /// <summary> Gets or sets the duration. </summary>
        /// <value> The duration. </value>
        public TimeSpan? Duration { get; set; }

        /// <summary> Gets or sets the action. </summary>
        /// <value> The action. </value>
        public UserDialogToastAction Action { get; set; }

        /// <summary> Gets or sets the icon. </summary>
        /// <value> The icon. </value>
        public IBitmap Icon { get; set; }

        /// <summary> Constructor. </summary>
        /// <param name="message"> The message. </param>
        public UserDialogToastConfig(string message)
        {
            Message = message;
        }

        /// <summary> Sets background color. </summary>
        /// <param name="color"> The color. </param>
        /// <returns> An UserDialogToastConfig. </returns>
        public UserDialogToastConfig SetBackgroundColor(Color color)
        {
            BackgroundColor = color;
            return this;
        }

        /// <summary> Sets a position. </summary>
        /// <param name="position"> The position. </param>
        /// <returns> An UserDialogToastConfig. </returns>
        public UserDialogToastConfig SetPosition(UserDialogToastPosition position)
        {
            Position = position;
            return this;
        }

        /// <summary> Sets a duration. </summary>
        /// <param name="millis"> The millis. </param>
        /// <returns> An UserDialogToastConfig. </returns>
        public UserDialogToastConfig SetDuration(int millis)
        {
            Duration = TimeSpan.FromMilliseconds(millis);
            return this;
        }

        /// <summary> Sets a duration. </summary>
        /// <param name="duration"> (Optional) The duration. </param>
        /// <returns> An UserDialogToastConfig. </returns>
        public UserDialogToastConfig SetDuration(TimeSpan? duration = null)
        {
            Duration = duration ?? Duration;
            return this;
        }

        /// <summary> Sets an action. </summary>
        /// <param name="action"> The action. </param>
        /// <returns> An UserDialogToastConfig. </returns>
        public UserDialogToastConfig SetAction(Action<UserDialogToastAction> action)
        {
            Action = new UserDialogToastAction();
            Action.SetAction(() => action.Invoke(Action));
            return this;
        }

        /// <summary> Sets an action. </summary>
        /// <param name="action"> The action. </param>
        /// <returns> An UserDialogToastConfig. </returns>
        public UserDialogToastConfig SetAction(UserDialogToastAction action)
        {
            Action = action;
            return this;
        }

        /// <summary> Sets message text color. </summary>
        /// <param name="color"> The color. </param>
        /// <returns> An UserDialogToastConfig. </returns>
        public UserDialogToastConfig SetMessageTextColor(Color color)
        {
            MessageTextColor = color;
            return this;
        }

        /// <summary> Sets an icon. </summary>
        /// <param name="icon"> The icon. </param>
        /// <returns> An UserDialogToastConfig. </returns>
        public UserDialogToastConfig SetIcon(IBitmap icon)
        {
            Icon = icon;
            return this;
        }
    }

    /// <summary> A user dialog toast action. </summary>
    public class UserDialogToastAction
    {
        /// <summary> Gets or sets the text. </summary>
        /// <value> The text. </value>
        public string Text { get; set; }

        /// <summary> Gets or sets the color of the text. </summary>
        /// <value> The color of the text. </value>
        public Color? TextColor { get; set; }

        /// <summary> Gets or sets the action. </summary>
        /// <value> The action. </value>
        public Action Action { get; set; }

        /// <summary> Sets a text. </summary>
        /// <param name="text"> The text. </param>
        /// <returns> An UserDialogToastAction. </returns>
        public UserDialogToastAction SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary> Sets text color. </summary>
        /// <param name="color"> The color. </param>
        /// <returns> An UserDialogToastAction. </returns>
        public UserDialogToastAction SetTextColor(Color color)
        {
            TextColor = color;
            return this;
        }

        /// <summary> Sets an action. </summary>
        /// <param name="action"> The action. </param>
        /// <returns> An UserDialogToastAction. </returns>
        public UserDialogToastAction SetAction(Action action)
        {
            Action = action;
            return this;
        }
    }
}
